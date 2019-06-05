# Shuttle.Core.Mediator

```
PM> Install-Package Shuttle.Core.Mediator
```

The Shuttle.Core.Mediator package provides an asynchronous mediator implementation using well established messaging semantics.

The `IMediator` interface contains the following methods that are implemented by the default `Mediator` implementation:

```c#
Task<TResponse> Request<TResponse>(object request, CancellationToken cancellationToken = default);
Task Send(object message, CancellationToken cancellationToken = default);
Task Publish(object message, CancellationToken cancellationToken = default);
```

The default `Mediator` has a dependency on an `IComponentResolver` implementation in order to resolve the various observers that implement the relevant message observer interfaces.

## Request/Response

When using the `Request<TResponse>` method a *single* observer will be resolved.  The observer must implement the `IRequestObserver` interface.

## Command

When using the `Send` method a *single* observer will be resolved.  The observer must implement the `IMessageObserver` interface.

## Event

When using the `Publish` method *all* observers that implement the relevant `IMessageObserver` will be resolved.