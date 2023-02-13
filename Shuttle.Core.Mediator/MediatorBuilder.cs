using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Mediator
{
    public class MediatorBuilder
    {
        private readonly Type _participantType = typeof(IParticipant<>);
        private readonly Type[] _participantTypeCollection = { typeof(IParticipant<>) };
        private readonly IServiceCollection _services;

        public MediatorBuilder(IServiceCollection services)
        {
            _services = Guard.AgainstNull(services, nameof(services));
        }

        public MediatorBuilder AddParticipants(Assembly assembly)
        {
            Guard.AgainstNull(assembly, nameof(assembly));

            var reflectionService = new ReflectionService();

            var implementationTypes = reflectionService.GetTypesAssignableTo(_participantType, assembly).GetAwaiter().GetResult();

            foreach (var type in implementationTypes)
            {
                var interfaces = type.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    if (@interface.Name != _participantType.Name)
                    {
                        continue;
                    }

                    _services.AddSingleton(_participantType.MakeGenericType(@interface.GetGenericArguments().First()), type);
                }
            }

            return this;
        }

        public MediatorBuilder AddParticipant<TParticipant>()
        {
            AddParticipant(typeof(TParticipant));

            return this;
        }

        public MediatorBuilder AddParticipant(Type participantType)
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

        public MediatorBuilder AddParticipant<TMessage>(IParticipant<TMessage> participant)
        {
            _services.AddSingleton(_participantType.MakeGenericType(typeof(TMessage)), participant);

            return this;
        }
    }
}