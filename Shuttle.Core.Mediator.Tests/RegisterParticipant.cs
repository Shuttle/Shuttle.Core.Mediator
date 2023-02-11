using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class RegisterParticipant : AbstractParticipant, IParticipant<RegisterMessage>
    {
        public async Task ProcessMessage(IParticipantContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[proper] : {Id}");

            await Call();
        }
    }
}