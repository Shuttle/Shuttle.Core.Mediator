using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class WriteParticipant : AbstractParticipant, IParticipant<WriteMessage>
    {
        public void ProcessMessage(IParticipantContext<WriteMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            Console.WriteLine($@"[command] : text = '{context.Message.Text}'");

            Call();
        }
    }
}