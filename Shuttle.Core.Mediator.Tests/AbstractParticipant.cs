using System;
using System.Threading.Tasks;

namespace Shuttle.Core.Mediator.Tests;

public abstract class AbstractParticipant
{
    public int CallCount { get; private set; }

    public Guid Id { get; } = Guid.NewGuid();
    public DateTimeOffset WhenCalled { get; private set; }

    public void Call()
    {
        CallCount++;
        WhenCalled = DateTimeOffset.UtcNow;
    }

    public async Task CallAsync()
    {
        Call();

        await Task.CompletedTask.ConfigureAwait(false);
    }
}