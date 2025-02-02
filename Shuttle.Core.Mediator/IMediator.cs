using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Core.Mediator;

public interface IMediator
{
    Task SendAsync(object message, CancellationToken cancellationToken = default);

    event EventHandler<SendEventArgs> Sending;
    event EventHandler<SendEventArgs> Sent;
}