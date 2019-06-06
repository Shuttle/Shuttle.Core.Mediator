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
        private static readonly Type BeforeObserverAttributeType = typeof(BeforeObserverAttribute);
        private static readonly Type AfterObserverAttributeType = typeof(AfterObserverAttribute);
        private static readonly Type ObserverContextType = typeof(ObserverContext<>);
        private static readonly Type MessageObserverType = typeof(IMessageObserver<>);
        private static readonly object Lock = new object();
        private readonly Dictionary<Type, Attributes> _attributes = new Dictionary<Type, Attributes>();
        private readonly Dictionary<Type, ContextMethod> _cache = new Dictionary<Type, ContextMethod>();

        private readonly IComponentResolver _resolver;

        public Mediator(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, nameof(resolver));

            _resolver = resolver;
        }

        public Task Send(object message, CancellationToken cancellationToken = default)
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

            var parameters = new[]
                {Activator.CreateInstance(contextMethod.ContextType, message, cancellationToken)};

            var invokes = new Dictionary<PipelineSequence, List<Action>>
            {
                {PipelineSequence.Before, new List<Action>()},
                {PipelineSequence.Proper, new List<Action>()},
                {PipelineSequence.After, new List<Action>()}
            };

            foreach (var observer in observers)
            {
                var type = observer.GetType();

                RegisterAttributes(type);

                var attributes = _attributes[type];

                void Action() => contextMethod.Method.Invoke(observer, parameters);

                if (attributes.HasBeforeAttribute)
                {
                    invokes[PipelineSequence.Before].Add(Action);
                }

                if (!attributes.HasBeforeAttribute && !attributes.HasAfterAttribute)
                {
                    invokes[PipelineSequence.Proper].Add(Action);
                }

                if (attributes.HasAfterAttribute)
                {
                    invokes[PipelineSequence.After].Add(Action);
                }
            }

            return Task.Run(() =>
            {
                foreach (var action in invokes[PipelineSequence.Before])
                {
                    action();
                }

                foreach (var action in invokes[PipelineSequence.Proper])
                {
                    action();
                }

                foreach (var action in invokes[PipelineSequence.After])
                {
                    action();
                }
            }, cancellationToken);
        }

        private void RegisterAttributes(Type type)
        {
            if (_attributes.ContainsKey(type))
            {
                return;
            }

            _attributes.Add(type, new Attributes
            {
                HasAfterAttribute = Attribute.IsDefined(type, AfterObserverAttributeType),
                HasBeforeAttribute = Attribute.IsDefined(type, BeforeObserverAttributeType)
            });
        }

        private ContextMethod GetContextMethod(Type type, Type messageType)
        {
            lock (Lock)
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

        private class ContextMethod
        {
            public Type ContextType { get; set; }
            public MethodInfo Method { get; set; }
        }

        private class Attributes
        {
            public bool HasBeforeAttribute { get; set; }
            public bool HasAfterAttribute { get; set; }
        }

        private enum PipelineSequence
        {
            Before,
            Proper,
            After
        }
    }
}