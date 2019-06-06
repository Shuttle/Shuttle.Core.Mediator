using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class WriteMessageObserver : Observer, IMessageObserver<WriteMessage>
    {
        public void ProcessMessage(IObserverContext<WriteMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            Console.WriteLine($@"[command] : text = '{context.Message.Text}'");

            Call();
        }
    }
}