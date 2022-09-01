using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class RegisterParticipant : AbstractParticipant, IParticipant<RegisterMessage>
    {
        public void ProcessMessage(IParticipantContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[proper] : {Id}");

            Call();
        }
    }
}