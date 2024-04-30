using System.Threading.Tasks;

namespace Shuttle.Core.Mediator
{
    public interface IAsyncParticipant<in T>
    {
        Task ProcessMessageAsync(IParticipantContext<T> context);
    }
}