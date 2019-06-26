using Shuttle.Core.Container;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator
{
    public static class ComponentResolverExtensions
    {
        /// <summary>
        ///     Adds any participant instances registered against the open generic `IParticipant`.
        /// </summary>
        /// <param name="resolver">The `IComponentResolver` instance to resolve dependencies from.</param>
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