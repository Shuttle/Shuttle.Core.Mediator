using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class MessageObserver : Observer, IMessageObserver<RegisterMessage>
    {
        public void ProcessMessage(IObserverContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[proper] : {Id}");

            Call();
        }
    }
}