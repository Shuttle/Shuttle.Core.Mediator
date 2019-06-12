using Shuttle.Core.Container;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator
{
    public static class ComponentResolverExtensions
    {
        public static void AddMediatorParticipants(this IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, nameof(resolver));

            var mediator = resolver.Resolve<IMediator>();

            foreach (var participant in resolver.ResolveAll(typeof(IParticipant<>)))
            {
                mediator.Add(participant);
            }
        }
    }
}