using System.Threading;

namespace Shuttle.Core.Mediator
{
    public interface IParticipantContext<out TRequest>
    {
        TRequest Message { get; }
        CancellationToken CancellationToken { get; }
    }
}