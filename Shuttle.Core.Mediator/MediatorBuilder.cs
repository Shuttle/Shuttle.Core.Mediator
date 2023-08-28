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
        private readonly Type _asyncParticipantType = typeof(IAsyncParticipant<>);
        private readonly IServiceCollection _services;

        public MediatorBuilder(IServiceCollection services)
        {
            _services = Guard.AgainstNull(services, nameof(services));
        }

        public MediatorBuilder AddParticipants(Assembly assembly)
        {
            Guard.AgainstNull(assembly, nameof(assembly));

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypesAssignableTo(_participantType, assembly))
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

            foreach (var type in reflectionService.GetTypesAssignableTo(_asyncParticipantType, assembly))
            {
                var interfaces = type.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    if (@interface.Name != _asyncParticipantType.Name)
                    {
                        continue;
                    }

                    _services.AddSingleton(_asyncParticipantType.MakeGenericType(@interface.GetGenericArguments().First()), type);
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
            var isParticipantType = false;

            if (participantType.IsAssignableTo(_participantType))
            {
                _services.AddSingleton(
                    _participantType.MakeGenericType(participantType.GetInterface(_participantType.Name)
                        .GetGenericArguments().First()), participantType);

                isParticipantType = true;
            }

            if (participantType.IsAssignableTo(_asyncParticipantType))
            {
                _services.AddSingleton(
                    _asyncParticipantType.MakeGenericType(participantType.GetInterface(_asyncParticipantType.Name)
                        .GetGenericArguments().First()), participantType);

                isParticipantType = true;
            }
            if (!isParticipantType)
            {
                throw new InvalidOperationException(string.Format(Resources.InvalidParticipantTypeException,
                    participantType.Name));
            }

            return this;
        }

        public MediatorBuilder AddParticipant<TMessage>(IParticipant<TMessage> participant)
        {
            _services.AddSingleton(_participantType.MakeGenericType(typeof(TMessage)), Guard.AgainstNull(participant, nameof(participant)));

            return this;
        }

        public MediatorBuilder AddParticipant<TMessage>(IAsyncParticipant<TMessage> participant)
        {
            _services.AddSingleton(_asyncParticipantType.MakeGenericType(typeof(TMessage)), Guard.AgainstNull(participant, nameof(participant)));

            return this;
        }
    }
}