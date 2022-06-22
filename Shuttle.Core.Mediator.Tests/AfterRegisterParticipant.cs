using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    [AfterObserver]
    public class AfterRegisterParticipant : AbstractParticipant, IParticipant<RegisterMessage>
    {
        public void ProcessMessage(IParticipantContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[after] : {Id}");

            Call();
        }
    }
}