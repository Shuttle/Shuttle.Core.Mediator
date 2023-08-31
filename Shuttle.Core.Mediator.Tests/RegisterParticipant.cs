using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class RegisterParticipant : AbstractParticipant, IParticipant<RegisterMessage>, IAsyncParticipant<RegisterMessage>
    {
        public void ProcessMessage(IParticipantContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[proper] : {Id}");

            Call();
        }

        public async Task ProcessMessageAsync(IParticipantContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[proper] : {Id}");

            await CallAsync();
        }
    }
}