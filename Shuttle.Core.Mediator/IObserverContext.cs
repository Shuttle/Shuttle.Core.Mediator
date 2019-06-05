using System.Threading;

namespace Shuttle.Core.Mediator
{
    public interface IObserverContext<out TRequest>
    {
        TRequest Message { get; }
        CancellationToken CancellationToken { get; }
    }
}