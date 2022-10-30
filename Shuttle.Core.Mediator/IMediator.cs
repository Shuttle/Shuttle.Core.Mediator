using System;
using System.Threading;

namespace Shuttle.Core.Mediator
{
    public interface IMediator
    {
        event EventHandler<SendEventArgs> Sending;
        event EventHandler<SendEventArgs> Sent;
        
        void Send(object message, CancellationToken cancellationToken = default);
    }
}