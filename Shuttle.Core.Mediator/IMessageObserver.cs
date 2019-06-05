namespace Shuttle.Core.Mediator
{
    public interface IMessageObserver<in T>
    {
        void ProcessMessage(IObserverContext<T> context);
    }
}