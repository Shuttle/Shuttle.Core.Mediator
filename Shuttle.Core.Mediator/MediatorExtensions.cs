using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator
{
    public static class MediatorExtensions
    {
        /// <summary>
        ///     Sends a message asynchronously.
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task SendAsync(this IMediator mediator, object message, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(mediator, nameof(mediator));
            Guard.AgainstNull(message, nameof(message));

            return new Task(()=> mediator.Send(message, cancellationToken), cancellationToken);
        }

        public static T Send<T>(this IMediator mediator, T message, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(mediator, nameof(mediator));
            Guard.AgainstNull(message, nameof(message));

            mediator.Send(message, cancellationToken);

            return message;
        }
    }
}