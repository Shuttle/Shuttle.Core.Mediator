using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    [BeforeObserver]
    public class BeforeMessageObserver : Observer, IMessageObserver<RegisterMessage>
    {
        public void ProcessMessage(IObserverContext<RegisterMessage> context)
        {
            Guard.AgainstNull(context, nameof(context));

            context.Message.Touch($"[before] : {Id}");

            Call();
        }
    }
}