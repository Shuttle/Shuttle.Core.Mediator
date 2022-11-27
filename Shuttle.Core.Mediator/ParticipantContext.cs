using System.Threading;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator
{
    public class ParticipantContext<TRequest> : IParticipantContext<TRequest>
    {
        public ParticipantContext(TRequest message, CancellationToken cancellationToken)
        {
            Message = Guard.AgainstNull(message, nameof(message));
            CancellationToken = cancellationToken;
        }

        public TRequest Message { get; }
        public CancellationToken CancellationToken { get; }
    }
}