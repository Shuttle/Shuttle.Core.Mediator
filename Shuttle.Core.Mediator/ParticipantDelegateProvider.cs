using System;
using System.Collections.Generic;

namespace Shuttle.Core.Mediator;

public class ParticipantDelegateProvider : IParticipantDelegateProvider
{
    public ParticipantDelegateProvider(IDictionary<Type, List<ParticipantDelegate>> participantDelegates)
    {
        Delegates = participantDelegates;
    }

    public IDictionary<Type, List<ParticipantDelegate>> Delegates { get; }
}