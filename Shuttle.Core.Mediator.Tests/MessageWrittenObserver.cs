using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class MessageWrittenObserver : IMessageObserver<MessageWritten>
    {
        private readonly Guid _id = Guid.NewGuid();
        public int CallCount { get; private set; }

        public void ProcessMessage(IObserverContext<MessageWritten> context)
        {
            Guard.AgainstNull(context, nameof(context));

            Console.WriteLine($@"[event-{_id}] : text = '{context.Message.Text}'");

            CallCount += 1;
        }
    }
}