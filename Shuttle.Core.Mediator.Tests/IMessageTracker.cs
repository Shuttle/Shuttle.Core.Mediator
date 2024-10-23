namespace Shuttle.Core.Mediator.Tests;

public interface IMessageTracker
{
    int MessageTypeCount<T>();
    void Received(object message);
}