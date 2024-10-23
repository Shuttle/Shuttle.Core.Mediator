using System;
using System.Threading;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator;

public class SendEventArgs : EventArgs
{
    public SendEventArgs(object message, CancellationToken cancellationToken = default)
    {
        Message = Guard.AgainstNull(message);
        CancellationToken = cancellationToken;
    }

    public CancellationToken CancellationToken { get; }
    public Guid Id { get; } = Guid.NewGuid();
    public object Message { get; }
}