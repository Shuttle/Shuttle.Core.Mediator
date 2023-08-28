using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class WriteAsyncParticipant : AbstractParticipant, IAsyncParticipant<WriteMessage>
    {
        public async Task ProcessMessage(IParticipantContext<WriteMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            Console.WriteLine($@"[command] : text = '{context.Message.Text}'");

            await CallAsync();
        }
    }
}