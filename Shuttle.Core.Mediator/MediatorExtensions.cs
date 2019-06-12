using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator
{
    public static class MediatorExtensions
    {
        public static Task SendAsync(this IMediator mediator, object message, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(mediator, nameof(mediator));
            Guard.AgainstNull(message, nameof(message));

            return new Task(()=> mediator.Send(message, cancellationToken), cancellationToken);
        }
    }
}