using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator
{
    public class Mediator : IMediator
    {
        private static readonly Type ObserverContextType = typeof(ObserverContext<>);
        private static readonly Type RequestObserverType = typeof(IRequestObserver<,>);
        private static readonly Type MessageObserverType = typeof(IMessageObserver<>);
        private static readonly object LockGetObserver = new object();
        private static readonly object LockInvoke = new object();
        private readonly Dictionary<Type, ContextMethod> _cache = new Dictionary<Type, ContextMethod>();

        private readonly Dictionary<Type, Dictionary<int, object>> _observers =
            new Dictionary<Type, Dictionary<int, object>>();

        private readonly IComponentResolver _resolver;

        public Mediator(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, nameof(resolver));

            _resolver = resolver;
        }

        public Task<TResponse> Request<TResponse>(object request,
            CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(request, nameof(request));

            var requestType = request.GetType();
            var responseType = typeof(TResponse);
            var interfaceType = RequestObserverType.MakeGenericType(requestType, responseType);
            var observer = GetObserver(interfaceType);

            try
            {
                var contextMethod = GetContextMethod(interfaceType, requestType);

                return Task.Run(
                    () => (TResponse) contextMethod.Method.Invoke(observer,
                        new[] {Activator.CreateInstance(contextMethod.ContextType, request, cancellationToken)}),
                    cancellationToken);
            }
            finally
            {
                if (observer is IReusability reusability && !reusability.IsReusable)
                {
                    ReleaseObserver(interfaceType);
                }
            }
        }

        public Task Send(object message, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(message, nameof(message));

            var messageType = message.GetType();
            var interfaceType = MessageObserverType.MakeGenericType(messageType);
            var observer = GetObserver(messageType);

            try
            {
                var contextMethod = GetContextMethod(interfaceType, messageType);

                return Task.Run(
                    () => contextMethod.Method.Invoke(observer,
                        new[] {Activator.CreateInstance(contextMethod.ContextType, message, cancellationToken)}),
                    cancellationToken);
            }
            finally
            {
                if (observer is IReusability reusability && !reusability.IsReusable)
                {
                    ReleaseObserver(messageType);
                }
            }
        }

        public Task Publish(object message, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(message, nameof(message));

            var messageType = message.GetType();
            var interfaceType = MessageObserverType.MakeGenericType(messageType);
            var observers = _resolver.ResolveAll(MessageObserverType.MakeGenericType(messageType)).ToList();

            if (!observers.Any())
            {
                return Task.CompletedTask;
            }

            var contextMethod = GetContextMethod(interfaceType, messageType);

            return Task.Run(() =>
            {
                foreach (var observer in observers)
                {
                    contextMethod.Method.Invoke(observer,
                        new[] {Activator.CreateInstance(contextMethod.ContextType, message, cancellationToken)});
                }
            }, cancellationToken);
        }

        private ContextMethod GetContextMethod(Type type, Type messageType)
        {
            lock (LockInvoke)
            {
                if (!_cache.TryGetValue(type, out var contextMethod))
                {
                    var method = type.GetMethods().SingleOrDefault();

                    if (method == null)
                    {
                        throw new InvalidOperationException(
                            string.Format(Resources.ProcessMessageMethodMissingException, type.FullName));
                    }

                    contextMethod = new ContextMethod
                    {
                        ContextType = ObserverContextType.MakeGenericType(messageType),
                        Method = method
                    };

                    _cache.Add(type, contextMethod);
                }

                return contextMethod;
            }
        }

        private void ReleaseObserver(Type interfaceType)
        {
            lock (LockGetObserver)
            {
                if (!_observers.TryGetValue(interfaceType, out var instances))
                {
                    return;
                }

                instances.Remove(Thread.CurrentThread.ManagedThreadId);
            }
        }

        private object GetObserver(Type interfaceType)
        {
            lock (LockGetObserver)
            {
                if (!_observers.TryGetValue(interfaceType, out var instances))
                {
                    instances = new Dictionary<int, object>();
                    _observers.Add(interfaceType, instances);
                }

                var managedThreadId = Thread.CurrentThread.ManagedThreadId;

                if (!instances.TryGetValue(managedThreadId, out var observer))
                {
                    observer = _resolver.Resolve(interfaceType);

                    if (observer == null)
                    {
                        throw new InvalidOperationException(string.Format(Resources.MissingObserverException,
                            interfaceType.FullName));
                    }

                    instances.Add(managedThreadId, observer);
                }

                return observer;
            }
        }

        internal class ContextMethod
        {
            public Type ContextType { get; set; }
            public MethodInfo Method { get; set; }
        }
    }
}