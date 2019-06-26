using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Mediator
{
    public class Mediator : IMediator
    {
        private static readonly Type BeforeParticipantAttributeType = typeof(BeforeObserverAttribute);
        private static readonly Type AfterParticipantAttributeType = typeof(AfterObserverAttribute);
        private static readonly Type ObserverContextType = typeof(ParticipantContext<>);
        private static readonly Type ObserverType = typeof(IParticipant<>);

        private readonly object _lock = new object();
        private readonly Dictionary<Type, ContextMethod> _cache = new Dictionary<Type, ContextMethod>();
        private readonly Dictionary<Type, Participants> _participants = new Dictionary<Type, Participants>();

        public IMediator Add(object participant)
        {
            Guard.AgainstNull(participant, nameof(participant));

            if (participant is IEnumerable<object> enumerable)
            {
                foreach (var o in enumerable)
                {
                    Add(o);
                }

                return this;
            }

            var type = participant.GetType();

            if (!type.IsAssignableTo(ObserverType))
            {
                throw new InvalidOperationException(string.Format(Resources.ParticipantInterfaceMissingException, type.FullName));
            }

            lock (_lock)
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (!interfaceType.IsAssignableTo(ObserverType))
                    {
                        continue;
                    }

                    if (!_participants.ContainsKey(interfaceType))
                    {
                        _participants.Add(interfaceType, new Participants());
                    }

                    _participants[interfaceType].Add(participant);
                }
            }

            return this;
        }

        public void Send(object message, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(message, nameof(message));

            var messageType = message.GetType();
            var interfaceType = ObserverType.MakeGenericType(messageType);

            if (!_participants.ContainsKey(interfaceType))
            {
                throw new InvalidOperationException(string.Format(Resources.MissingParticipantException, messageType));
            }

            var contextMethod = GetContextMethod(interfaceType, messageType);
            var parameters = new[]
                {Activator.CreateInstance(contextMethod.ContextType, message, cancellationToken)};

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
            lock (_lock)
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

        private enum FilterSequence
        {
            Before,
            Actual,
            After
        }

        private class Participants
        {
            private readonly List<object> _before = new List<object>();
            private readonly List<object> _actual = new List<object>();
            private readonly List<object> _after = new List<object>();

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