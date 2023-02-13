using System;
using System.Threading.Tasks;

namespace Shuttle.Core.Mediator.Tests
{
    public abstract class AbstractParticipant
    {
        protected AbstractParticipant()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public int CallCount { get; private set; }
        public DateTime WhenCalled { get; private set; }

        public async Task Call()
        {
            CallCount++;
            WhenCalled = DateTime.Now;

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}