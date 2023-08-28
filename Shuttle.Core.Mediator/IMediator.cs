using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Core.Mediator
{
    public interface IMediator
    {
        event EventHandler<SendEventArgs> Sending;
        event EventHandler<SendEventArgs> Sent;

        void Send(object message, CancellationToken cancellationToken = default);

        Task SendAsync(object message, CancellationToken cancellationToken = default);
    }
}