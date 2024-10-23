using System.Threading.Tasks;

namespace Shuttle.Core.Mediator;

public interface IParticipant<in T>
{
    Task ProcessMessageAsync(IParticipantContext<T> context);
}