using System;

namespace Shuttle.Core.Mediator.Tests
{
    public abstract class Observer
    {
        protected Observer()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public int CallCount { get; private set; }

        public void Call()
        {
            CallCount++;
        }
    }
}