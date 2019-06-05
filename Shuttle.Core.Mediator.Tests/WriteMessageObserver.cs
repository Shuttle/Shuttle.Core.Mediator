using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class WriteMessageObserver : IMessageObserver<WriteMessage>
    {
        public int CallCount { get; private set; }

        public void ProcessMessage(IObserverContext<WriteMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            Console.WriteLine($@"[command] : text = '{context.Message.Text}'");

            CallCount += 1;
        }
    }
}