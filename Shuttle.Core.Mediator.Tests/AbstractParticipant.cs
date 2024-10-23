using System;
using System.Threading.Tasks;

namespace Shuttle.Core.Mediator.Tests;

public abstract class AbstractParticipant
{
    protected AbstractParticipant()
    {
        Id = Guid.NewGuid();
    }

    public int CallCount { get; private set; }

    public Guid Id { get; }
    public DateTime WhenCalled { get; private set; }

    public void Call()
    {
        CallCount++;
        WhenCalled = DateTime.Now;
    }

    public async Task CallAsync()
    {
        Call();

        await Task.CompletedTask.ConfigureAwait(false);
    }
}