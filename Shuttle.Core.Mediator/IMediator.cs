using System.Threading;

namespace Shuttle.Core.Mediator
{
    public interface IMediator
    {
        IMediator Add(object participant);
        void Send(object message, CancellationToken cancellationToken = default);
    }
}