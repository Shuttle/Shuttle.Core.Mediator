using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    [AfterParticipant]
    public class AfterRegisterAsyncParticipant : AbstractParticipant, IAsyncParticipant<RegisterMessage>
    {
        public async Task ProcessMessage(IParticipantContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[after] : {Id}");

            await CallAsync();
        }
    }
}