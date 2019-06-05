namespace Shuttle.Core.Mediator
{
    public interface IRequestObserver<in TRequest, out TResponse>
    {
        TResponse ProcessMessage(IObserverContext<TRequest> context);
    }
}