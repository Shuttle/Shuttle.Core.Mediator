using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Mediator
{
    public class MediatorOptions
    {
        private readonly Type _participantType = typeof(IParticipant<>);
        private readonly IServiceCollection _services;

        public MediatorOptions(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            _services = services;
        }

        public MediatorOptions AddParticipants(Assembly assembly)
        {
            Guard.AgainstNull(assembly, nameof(assembly));

            var reflectionService = new ReflectionService();

            var implementationTypes = reflectionService.GetTypesAssignableTo(_participantType, assembly);

            foreach (var grouping in implementationTypes.GroupBy(item =>
                         item.GetInterface(_participantType.Name).GetGenericArguments().First()))
            {
                _services.AddSingleton(_participantType.MakeGenericType(grouping.Key), grouping);
            }

            return this;
        }

        public MediatorOptions AddParticipant<TParticipant>()
        {
            AddParticipant(typeof(TParticipant));

            return this;
        }

        public MediatorOptions AddParticipant(Type participantType)
        {
            if (!participantType.IsAssignableTo(_participantType))
            {
                throw new InvalidOperationException(string.Format(Resources.InvalidParticipantTypeException,
                    participantType.Name));
            }

            _services.AddSingleton(
                _participantType.MakeGenericType(participantType.GetInterface(_participantType.Name)
                    .GetGenericArguments().First()), participantType);

            return this;
        }

        public MediatorOptions AddParticipant<TMessage>(IParticipant<TMessage> participant)
        {
            _services.AddSingleton(_participantType.MakeGenericType(typeof(TMessage)), participant);

            return this;
        }
    }
}