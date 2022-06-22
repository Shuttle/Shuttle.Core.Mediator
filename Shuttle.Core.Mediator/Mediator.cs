using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator
{
    public class Mediator : IMediator
    {
        private static readonly Type BeforeParticipantAttributeType = typeof(BeforeObserverAttribute);
        private static readonly Type AfterParticipantAttributeType = typeof(AfterObserverAttribute);
        private static readonly Type ParticipantContextType = typeof(ParticipantContext<>);
        private static readonly Type ParticipantType = typeof(IParticipant<>);

        private static readonly object Lock = new();
        private readonly Dictionary<Type, ContextMethod> _cache = new();
        private readonly Dictionary<Type, Participants> _participants = new();
        private readonly IServiceProvider _provider;

        public Mediator(IServiceProvider provider)
        {
            Guard.AgainstNull(provider, nameof(provider));

            _provider = provider;
        }

        public void Send(object message, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(message, nameof(message));

            var messageType = message.GetType();
            var interfaceType = ParticipantType.MakeGenericType(messageType);

            if (!_participants.ContainsKey(interfaceType))
            {
                lock (Lock)
                {
                    _participants.Add(interfaceType, new Participants(_provider.GetServices(interfaceType)));
                }
            }

            var contextMethod = GetContextMethod(interfaceType, messageType);
            var parameters = new[]
                { Activator.CreateInstance(contextMethod.ContextType, message, cancellationToken) };

            var participants = _participants[interfaceType];

            foreach (var participant in participants.Get(FilterSequence.Before))
            {
                contextMethod.Method.Invoke(participant, parameters);
            }

            if (!participants.Get(FilterSequence.Actual).Any())
            {
                throw new InvalidOperationException(string.Format(Resources.MissingParticipantException, messageType));
            }

            foreach (var participant in participants.Get(FilterSequence.Actual))
            {
                contextMethod.Method.Invoke(participant, parameters);
            }

            foreach (var participant in participants.Get(FilterSequence.After))
            {
                contextMethod.Method.Invoke(participant, parameters);
            }
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
                        ContextType = ParticipantContextType.MakeGenericType(messageType),
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
                Guard.AgainstNull(participants, nameof(participants));

                foreach (var participant in participants)
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
}