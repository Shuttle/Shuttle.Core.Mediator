using System;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class RequestObserver : IRequestObserver<RequestMessage, ResponseMessage>
    {
        public int CallCount { get; private set; }

        public ResponseMessage ProcessMessage(IObserverContext<RequestMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            Console.WriteLine($@"[request] : text = '{context.Message.RequestText}'");

            CallCount += 1;

            return new ResponseMessage
            {
                ResponseText = new string(context.Message.RequestText.Reverse().ToArray())
            };
        }
    }
}