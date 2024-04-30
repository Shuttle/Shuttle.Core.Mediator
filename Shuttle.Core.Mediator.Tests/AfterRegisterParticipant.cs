using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    [AfterParticipant]
    public class AfterRegisterParticipant : AbstractParticipant, IParticipant<RegisterMessage>, IAsyncParticipant<RegisterMessage>
    {
        public void ProcessMessage(IParticipantContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[after] : {Id}");

            Call();
        }

        public async Task ProcessMessageAsync(IParticipantContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[after] : {Id}");

            await CallAsync();
        }
    }
}