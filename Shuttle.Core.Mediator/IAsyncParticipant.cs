using System.Threading.Tasks;

namespace Shuttle.Core.Mediator
{
    public interface IAsyncParticipant<in T>
    {
        Task ProcessMessage(IParticipantContext<T> context);
    }
}