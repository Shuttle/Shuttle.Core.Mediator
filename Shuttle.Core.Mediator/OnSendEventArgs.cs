using System;
using System.Threading;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator;

public class SendEventArgs : EventArgs
{
    public Guid Id { get; } = Guid.NewGuid();
    public object Message { get; }
    public CancellationToken CancellationToken { get; }

    public SendEventArgs(object message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message, nameof(message));

        Message = message;
        CancellationToken = cancellationToken;
    }
}