using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    [AfterObserver]
    public class AfterMessageObserver : Observer, IMessageObserver<RegisterMessage>
    {
        public void ProcessMessage(IObserverContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[after] : {Id}");

            Call();
        }
    }
}