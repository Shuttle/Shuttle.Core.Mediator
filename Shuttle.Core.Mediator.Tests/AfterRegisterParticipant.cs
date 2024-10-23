using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests;

[AfterParticipant]
public class AfterRegisterParticipant : AbstractParticipant, IParticipant<RegisterMessage>
{
    public async Task ProcessMessageAsync(IParticipantContext<RegisterMessage> context)
    {
        Guard.AgainstNull(context);

        context.Message.Touch($"[after] : {Id}");

        await CallAsync();
    }
}