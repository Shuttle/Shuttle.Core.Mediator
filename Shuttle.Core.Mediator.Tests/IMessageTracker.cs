namespace Shuttle.Core.Mediator.Tests;

public interface IMessageTracker
{
    void Received(object message);
    int MessageTypeCount<T>();
}