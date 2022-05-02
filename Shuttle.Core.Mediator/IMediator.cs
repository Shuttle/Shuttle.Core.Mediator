using System.Threading;

namespace Shuttle.Core.Mediator
{
    public interface IMediator
    {
        void Send(object message, CancellationToken cancellationToken = default);
    }
}