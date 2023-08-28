using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator
{
    public class Mediator : IMediator
    {
        private static readonly Type BeforeParticipantAttributeType = typeof(BeforeParticipantAttribute);
        private static readonly Type AfterParticipantAttributeType = typeof(AfterParticipantAttribute);
        private static readonly Type AsyncParticipantType = typeof(IAsyncParticipant<>);
        private static readonly Type ParticipantType = typeof(IParticipant<>);
        private readonly Dictionary<string, ContextMethodInvokerAsync> _methodCacheAsync = new Dictionary<string, ContextMethodInvokerAsync>();
        private readonly Dictionary<string, ContextMethodInvoker> _methodCache = new Dictionary<string, ContextMethodInvoker>();
        private readonly Dictionary<Type, ContextConstructorInvoker> _constructorCache = new Dictionary<Type, ContextConstructorInvoker>();

        private static readonly object Lock = new();
        private readonly Dictionary<Type, Participants> _participants = new();
        private readonly IServiceProvider _provider;

        public event EventHandler<SendEventArgs> Sending = delegate
        {
        };

        public event EventHandler<SendEventArgs> Sent = delegate
        {
        };

        public Mediator(IServiceProvider provider)
        {
            _provider = Guard.AgainstNull(provider, nameof(provider));
        }

        public void Send(object message, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(message, nameof(message));

            var onSendEventArgs = new SendEventArgs(message, cancellationToken);

            Sending.Invoke(this, onSendEventArgs);

            var messageType = message.GetType();
            var interfaceType = ParticipantType.MakeGenericType(messageType);

            if (!_participants.ContainsKey(interfaceType))
            {
                lock (Lock)
                {
                    _participants.Add(interfaceType, new Participants(_provider.GetServices(interfaceType)));
                }
            }

            var participants = _participants[interfaceType];

            if (!participants.Get(FilterSequence.Actual).Any())
            {
                throw new InvalidOperationException(string.Format(Resources.MissingParticipantException, messageType));
            }

            ContextConstructorInvoker contextConstructor;

            lock (Lock)
            {
                if (!_constructorCache.TryGetValue(messageType, out contextConstructor))
                {
                    contextConstructor = new ContextConstructorInvoker(messageType);

                    _constructorCache.Add(messageType, contextConstructor);
                }
            }

            var participantContext = contextConstructor.CreateParticipantContext(message, cancellationToken);

            foreach (var participant in participants.Get(FilterSequence.Before))
            {
                GetContextMethodInvoker(participant.GetType(), messageType, interfaceType).Invoke(participant, participantContext);
            }

            foreach (var participant in participants.Get(FilterSequence.Actual))
            {
                GetContextMethodInvoker(participant.GetType(), messageType, interfaceType).Invoke(participant, participantContext);
            }

            foreach (var participant in participants.Get(FilterSequence.After))
            {
                GetContextMethodInvoker(participant.GetType(), messageType, interfaceType).Invoke(participant, participantContext);
            }

            Sent.Invoke(this, onSendEventArgs);
        }

        public async Task SendAsync(object message, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(message, nameof(message));

            var onSendEventArgs = new SendEventArgs(message,cancellationToken);

            Sending.Invoke(this, onSendEventArgs);

            var messageType = message.GetType();
            var interfaceType = AsyncParticipantType.MakeGenericType(messageType);

            if (!_participants.ContainsKey(interfaceType))
            {
                lock (Lock)
                {
                    _participants.Add(interfaceType, new Participants(_provider.GetServices(interfaceType)));
                }
            }

            var participants = _participants[interfaceType];

            if (!participants.Get(FilterSequence.Actual).Any())
            {
                throw new InvalidOperationException(string.Format(Resources.MissingParticipantException, messageType));
            }

            ContextConstructorInvoker contextConstructor;

            lock (Lock)
            {
                if (!_constructorCache.TryGetValue(messageType, out contextConstructor))
                {
                    contextConstructor = new ContextConstructorInvoker(messageType);

                    _constructorCache.Add(messageType, contextConstructor);
                }
            }

            var participantContext = contextConstructor.CreateParticipantContext(message, cancellationToken);

            foreach (var participant in participants.Get(FilterSequence.Before))
            {
                await GetContextMethodInvokerAsync(participant.GetType(), messageType, interfaceType).Invoke(participant, participantContext).ConfigureAwait(false);
            }

            foreach (var participant in participants.Get(FilterSequence.Actual))
            {
                await GetContextMethodInvokerAsync(participant.GetType(), messageType, interfaceType).Invoke(participant, participantContext).ConfigureAwait(false);
            }

            foreach (var participant in participants.Get(FilterSequence.After))
            {
                await GetContextMethodInvokerAsync(participant.GetType(), messageType, interfaceType).Invoke(participant, participantContext).ConfigureAwait(false);
            }

            Sent.Invoke(this, onSendEventArgs);
        }

        private ContextMethodInvoker GetContextMethodInvoker(Type participantType, Type messageType, Type interfaceType)
        {
            lock (Lock)
            {
                var key = $"{participantType.Name}:{messageType.Name}";

                if (!_methodCache.TryGetValue(key, out var contextMethod))
                {
                    var method = participantType.GetInterfaceMap(interfaceType)
                        .TargetMethods.SingleOrDefault();

                    if (method == null)
                    {
                        throw new InvalidOperationException(string.Format(
                            Resources.ProcessMessageMethodMissingException,
                            participantType.FullName,
                            messageType.FullName));
                    }

                    contextMethod = new ContextMethodInvoker(
                        participantType
                            .GetInterfaceMap(ParticipantType.MakeGenericType(messageType))
                            .TargetMethods.SingleOrDefault()
                    );

                    _methodCache.Add(key, contextMethod);
                }

                return contextMethod;
            }
        }

        private ContextMethodInvokerAsync GetContextMethodInvokerAsync(Type participantType, Type messageType, Type interfaceType)
        {
            lock (Lock)
            {
                var key = $"{participantType.Name}:{messageType.Name}";

                if (!_methodCacheAsync.TryGetValue(key, out var contextMethod))
                {
                    var method = participantType.GetInterfaceMap(interfaceType)
                        .TargetMethods.SingleOrDefault();

                    if (method == null)
                    {
                        throw new InvalidOperationException(string.Format(
                            Resources.ProcessMessageMethodMissingException,
                            participantType.FullName,
                            messageType.FullName));
                    }

                    contextMethod = new ContextMethodInvokerAsync(
                        participantType
                            .GetInterfaceMap(AsyncParticipantType.MakeGenericType(messageType))
                            .TargetMethods.SingleOrDefault()
                    );

                    _methodCacheAsync.Add(key, contextMethod);
                }

                return contextMethod;
            }
        }

        private enum FilterSequence
        {
            Before,
            Actual,
            After
        }

        private class Participants
        {
            private readonly List<object> _actual = new();
            private readonly List<object> _after = new();
            private readonly List<object> _before = new();

            public Participants(IEnumerable<object> participants)
            {
                foreach (var participant in Guard.AgainstNull(participants, nameof(participants)))
                {
                    Add(participant);
                }
            }

            public IEnumerable<object> Get(FilterSequence sequence)
            {
                switch (sequence)
                {
                    case FilterSequence.Before:
                    {
                        return _before;
                    }
                    case FilterSequence.After:
                    {
                        return _after;
                    }
                    default:
                    {
                        return _actual;
                    }
                }
            }

            public void Add(object participant)
            {
                if (participant == null)
                {
                    return;
                }

                var type = participant.GetType();

                var hasBefore = Attribute.IsDefined(type, BeforeParticipantAttributeType);
                var hasAfter = Attribute.IsDefined(type, AfterParticipantAttributeType);

                if (hasBefore)
                {
                    _before.Add(participant);
                }

                if (hasAfter)
                {
                    _after.Add(participant);
                }

                if (!hasBefore && !hasAfter)
                {
                    _actual.Add(participant);
                }
            }
        }
    }

    internal class ContextConstructorInvoker
    {
        private static readonly Type ParticipantContextType = typeof(ParticipantContext<>);

        private readonly ConstructorInvokeHandler _constructorInvoker;

        public ContextConstructorInvoker(Type messageType)
        {
            var dynamicMethod = new DynamicMethod(string.Empty, typeof(object),
                new[]
                {
                    typeof(object),
                    typeof(CancellationToken)
                }, ParticipantContextType.Module);

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);

            var contextType = ParticipantContextType.MakeGenericType(messageType);
            var constructorInfo = contextType.GetConstructor(new[]
            {
                messageType,
                typeof(CancellationToken)
            });

            if (constructorInfo == null)
            {
                throw new InvalidOperationException(string.Format(Resources.ContextConstructorException, contextType.FullName));
            }

            il.Emit(OpCodes.Newobj, constructorInfo);
            il.Emit(OpCodes.Ret);

            _constructorInvoker =
                (ConstructorInvokeHandler)dynamicMethod.CreateDelegate(typeof(ConstructorInvokeHandler));
        }

        public object CreateParticipantContext(object message, CancellationToken cancellationToken)
        {
            return _constructorInvoker(message, cancellationToken);
        }

        private delegate object ConstructorInvokeHandler(object message, CancellationToken cancellationToken);
    }

    internal class ContextMethodInvoker
    {
        private static readonly Type ParticipantContextType = typeof(ParticipantContext<>);

        private readonly InvokeHandler _invoker;

        public ContextMethodInvoker(MethodInfo methodInfo)
        {
            var dynamicMethod = new DynamicMethod(string.Empty,
                typeof(void), new[] { typeof(object), typeof(object) },
                ParticipantContextType.Module);

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);

            il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            il.Emit(OpCodes.Ret);

            _invoker = (InvokeHandler)dynamicMethod.CreateDelegate(typeof(InvokeHandler));
        }

        public void Invoke(object participant, object participantContext)
        {
            _invoker.Invoke(participant, participantContext);
        }

        private delegate void InvokeHandler(object handler, object handlerContext);
    }

    internal class ContextMethodInvokerAsync
    {
        private static readonly Type ParticipantContextType = typeof(ParticipantContext<>);

        private readonly InvokeHandler _invoker;

        public ContextMethodInvokerAsync(MethodInfo methodInfo)
        {
            var dynamicMethod = new DynamicMethod(string.Empty,
                typeof(Task), new[] { typeof(object), typeof(object) },
                ParticipantContextType.Module);

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);

            il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            il.Emit(OpCodes.Ret);

            _invoker = (InvokeHandler)dynamicMethod.CreateDelegate(typeof(InvokeHandler));
        }

        public async Task Invoke(object participant, object participantContext)
        {
            await _invoker.Invoke(participant, participantContext);
        }

        private delegate Task InvokeHandler(object handler, object handlerContext);
    }
}