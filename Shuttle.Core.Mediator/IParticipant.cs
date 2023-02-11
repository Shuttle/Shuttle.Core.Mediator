using System.Threading.Tasks;

namespace Shuttle.Core.Mediator
{
    public interface IParticipant<in T>
    {
        Task ProcessMessage(IParticipantContext<T> context);
    }
}