using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Mediator;

public class MediatorBuilder
{
    private readonly Type _participantType = typeof(IParticipant<>);
    private readonly IServiceCollection _services;

    public MediatorBuilder(IServiceCollection services)
    {
        _services = Guard.AgainstNull(services);
    }

    public MediatorBuilder AddParticipant<TParticipant>()
    {
        AddParticipant(typeof(TParticipant));

        return this;
    }

    public MediatorBuilder AddParticipant(Type participantType)
    {
        var isParticipantType = false;

        if (participantType.IsCastableTo(_participantType)) 
        {
            var participantInterface = participantType.GetInterface(_participantType.Name);

            if (participantInterface == null)
            {
                throw new InvalidOperationException(string.Format(Resources.InvalidParticipantTypeException, participantType.Name));
            }

            _services.AddSingleton(_participantType.MakeGenericType(participantInterface.GetGenericArguments().First()), participantType);

            isParticipantType = true;
        }

        if (!isParticipantType)
        {
            throw new InvalidOperationException(string.Format(Resources.InvalidParticipantTypeException, participantType.Name));
        }

        return this;
    }

    public MediatorBuilder AddParticipant<TMessage>(IParticipant<TMessage> participant)
    {
        _services.AddSingleton(_participantType.MakeGenericType(typeof(TMessage)), Guard.AgainstNull(participant));

        return this;
    }

    public MediatorBuilder AddParticipants(Assembly assembly)
    {
        Guard.AgainstNull(assembly);

        var reflectionService = new ReflectionService();

        foreach (var type in reflectionService.GetTypesCastableToAsync(_participantType, assembly).GetAwaiter().GetResult())
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
}