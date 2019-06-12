using System;
using System.Threading;

namespace Shuttle.Core.Mediator.Tests
{
    public abstract class AbstractObserver
    {
        protected AbstractObserver()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public int CallCount { get; private set; }
        public DateTime WhenCalled { get; private set; }

        public void Call()
        {
            CallCount++;
            WhenCalled = DateTime.Now;

            Thread.Sleep(1);
        }
    }
}