using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    [BeforeParticipant]
    public class BeforeRegisterParticipant : AbstractParticipant, IParticipant<RegisterMessage>, IAsyncParticipant<RegisterMessage>
    {
        public void ProcessMessage(IParticipantContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[before] : {Id}");

            Call();
        }

        public async Task ProcessMessageAsync(IParticipantContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[before] : {Id}");

            await CallAsync();
        }
    }
}