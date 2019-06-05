using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Core.Mediator
{
    public interface IMediator
    {
        Task<TResponse> Request<TResponse>(object request, CancellationToken cancellationToken = default);
        Task Send(object message, CancellationToken cancellationToken = default);
        Task Publish(object message, CancellationToken cancellationToken = default);
    }
}