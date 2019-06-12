namespace Shuttle.Core.Mediator
{
    public interface IParticipant<in T>
    {
        void ProcessMessage(IParticipantContext<T> context);
    }
}