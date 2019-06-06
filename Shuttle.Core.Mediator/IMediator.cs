using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Core.Mediator
{
    public interface IMediator
    {
        Task Send(object message, CancellationToken cancellationToken = default);
    }
}