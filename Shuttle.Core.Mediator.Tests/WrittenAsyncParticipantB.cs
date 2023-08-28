using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class WrittenAsyncParticipantB : AbstractParticipant, IAsyncParticipant<MessageWritten>
    {
        private readonly Guid _id = Guid.NewGuid();

        public async Task ProcessMessage(IParticipantContext<MessageWritten> context)
        {
            Guard.AgainstNull(context, nameof(context));

            Console.WriteLine($@"[event-{_id}] : text = '{context.Message.Text}'");

            await CallAsync();
        }
    }
}