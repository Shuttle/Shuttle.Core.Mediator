using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Mediator;

public class MediatorBuilder
{
    public IServiceCollection Services { get; }

    private readonly Dictionary<Type, List<ParticipantDelegate>> _delegates = new();
    private static readonly Type ParticipantType = typeof(IParticipant<>);
    private static readonly Type ParticipantContextType = typeof(IParticipantContext<>);

    public MediatorBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public IDictionary<Type, List<ParticipantDelegate>> GetDelegates() => new ReadOnlyDictionary<Type, List<ParticipantDelegate>>(_delegates);

    public MediatorBuilder AddParticipant<TParticipant>()
    {
        AddParticipant(typeof(TParticipant));

        return this;
    }

    public MediatorBuilder AddParticipant(Type participantType)
    {
        var isParticipantType = false;

        if (participantType.IsCastableTo(ParticipantType)) 
        {
            var participantInterface = participantType.GetInterface(ParticipantType.Name);

            if (participantInterface == null)
            {
                throw new InvalidOperationException(string.Format(Resources.InvalidParticipantTypeException, participantType.Name));
            }

            Services.AddSingleton(ParticipantType.MakeGenericType(participantInterface.GetGenericArguments().First()), participantType);

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
        Services.AddSingleton(ParticipantType.MakeGenericType(typeof(TMessage)), Guard.AgainstNull(participant));

        return this;
    }

    public MediatorBuilder AddParticipants(Assembly assembly)
    {
        Guard.AgainstNull(assembly);

        var reflectionService = new ReflectionService();

        foreach (var type in reflectionService.GetTypesCastableToAsync(ParticipantType, assembly).GetAwaiter().GetResult())
        {
            var interfaces = type.GetInterfaces();

            foreach (var @interface in interfaces)
            {
                if (@interface.Name != ParticipantType.Name)
                {
                    continue;
                }

                Services.AddSingleton(ParticipantType.MakeGenericType(@interface.GetGenericArguments().First()), type);
            }
        }

        return this;
    }

    public MediatorBuilder MapParticipant(Delegate handler)
    {
        if (!typeof(Task).IsAssignableFrom(Guard.AgainstNull(handler).Method.ReturnType))
        {
            throw new ApplicationException(Resources.AsyncDelegateRequiredException);
        }

        var parameters = handler.Method.GetParameters();
        Type? messageType = null;

        foreach (var parameter in parameters)
        {
            var parameterType = parameter.ParameterType;

            if (parameterType.IsCastableTo(ParticipantContextType))
            {
                messageType = parameterType.GetGenericArguments()[0];
            }
        }

        if (messageType == null)
        {
            throw new ApplicationException(Resources.ParticipantTypeException);
        }

        _delegates.TryAdd(messageType, new());
        _delegates[messageType].Add(new(handler, handler.Method.GetParameters().Select(item => item.ParameterType)));

        return this;
    }
}