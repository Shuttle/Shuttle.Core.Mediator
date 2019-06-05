using System.Threading;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator
{
    public class ObserverContext<TRequest> : IObserverContext<TRequest>
    {
        public ObserverContext(TRequest message, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(message, nameof(message));

            Message = message;
            CancellationToken = cancellationToken;
        }

        public TRequest Message { get; }
        public CancellationToken CancellationToken { get; }
    }
}