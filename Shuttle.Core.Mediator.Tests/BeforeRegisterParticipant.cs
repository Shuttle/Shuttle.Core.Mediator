using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests;

[BeforeParticipant]
public class BeforeRegisterParticipant : AbstractParticipant, IParticipant<RegisterMessage>
{
    public async Task ProcessMessageAsync(IParticipantContext<RegisterMessage> context)
    {
        Guard.AgainstNull(context);

        context.Message.Touch($"[before] : {Id}");

        await CallAsync();
    }
}