using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests;

public class MultipleParticipants :
    IParticipant<MultipleParticipantMessageA>,
    IParticipant<MultipleParticipantMessageB>
{
    private readonly IMessageTracker _messageTracker;

    public MultipleParticipants(IMessageTracker messageTracker)
    {
        _messageTracker = Guard.AgainstNull(messageTracker);
    }

    public async Task ProcessMessageAsync(IParticipantContext<MultipleParticipantMessageA> context)
    {
        _messageTracker.Received(context.Message);

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task ProcessMessageAsync(IParticipantContext<MultipleParticipantMessageB> context)
    {
        _messageTracker.Received(context.Message);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}