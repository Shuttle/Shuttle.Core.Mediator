using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Mediator;

public class ParticipantDelegate
{
    private readonly IEnumerable<Type> _parameterTypes;
    private static readonly Type ParticipantContextType = typeof(IParticipantContext<>);

    public ParticipantDelegate(Delegate handler, IEnumerable<Type> parameterTypes)
    {
        Handler = handler;
        HasParameters = parameterTypes.Any();
        _parameterTypes = parameterTypes;
    }

    public Delegate Handler { get; }
    public bool HasParameters { get; }

    public object[] GetParameters(IServiceProvider serviceProvider, object handlerContext)
    {
        return _parameterTypes
            .Select(parameterType => !parameterType.IsCastableTo(ParticipantContextType)
                ? serviceProvider.GetRequiredService(parameterType)
                : handlerContext
            ).ToArray();
    }
}