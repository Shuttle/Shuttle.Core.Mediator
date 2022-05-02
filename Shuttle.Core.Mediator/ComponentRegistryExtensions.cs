using System.Linq;
using System.Reflection;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Mediator
{
    public static class ComponentRegistryExtensions
    {
        public static void RegisterMediator(this IComponentRegistry registry)
        {
            Guard.AgainstNull(registry, nameof(registry));

            registry.AttemptRegister<IMediator, Mediator>();
        }

        /// <summary>
        ///     Registers all types that implement the `IParticipant<T>` interface against the open generic type `IParticipant<>`.
        /// </summary>
        /// <param name="registry">The `IComponentRegistry` instance to register the mapping against.</param>
        /// <param name="assembly">The assembly that contains the types to evaluate.</param>
        public static void RegisterMediatorParticipants(this IComponentRegistry registry, Assembly assembly)
        {
            Guard.AgainstNull(registry, nameof(registry));
            Guard.AgainstNull(assembly, nameof(assembly));

            var reflectionService = new ReflectionService();
            var participantType = typeof(IParticipant<>);

            var implementationTypes = reflectionService.GetTypesAssignableTo(participantType, assembly);

            foreach (var grouping in implementationTypes.GroupBy(item => item.GetInterface(participantType.Name).GetGenericArguments().First()))
            {
                registry.RegisterCollection(participantType.MakeGenericType(grouping.Key), grouping, Lifestyle.Singleton);
            }
        }
    }
}