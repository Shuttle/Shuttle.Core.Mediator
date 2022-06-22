using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    [BeforeObserver]
    public class BeforeRegisterParticipant : AbstractParticipant, IParticipant<RegisterMessage>
    {
        public void ProcessMessage(IParticipantContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[before] : {Id}");

            Call();
        }
    }
}