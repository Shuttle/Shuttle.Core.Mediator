# Shuttle.Core.Mediator

```
PM> Install-Package Shuttle.Core.Mediator
```

The Shuttle.Core.Mediator package provides an asynchronous mediator implementation.

```c#
Task Send(object message, CancellationToken cancellationToken = default);
```

The default `Mediator` has a dependency on an `IComponentResolver` implementation in order to resolve the various observers that implement the `IMessageObserver` interface.

## IMessageObserver

```c#
public interface IMessageObserver<in T>
{
    void ProcessMessage(IObserverContext<T> context);
}
```

An observer would handle the message that is sent on using the mediator.

There are not *request/response* semantics and the design philosophy here is that the message encapsulates the state that is passed along in a *pipes & filters* approach.

There may be *N* observers of the message.  In addition observers may be marked with the `[BeforeObserverAttribute]` attribute if the observer runs before the observer proper that does the message handling.  An observer may also be marked with the `[AfterObserverAttribute]` in order to have the observer handle the message after the observer proper has completed the message handling.