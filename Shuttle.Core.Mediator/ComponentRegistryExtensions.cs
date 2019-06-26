using System.Reflection;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Mediator
{
    public static class ComponentRegistryExtensions
    {
        /// <summary>
        ///     Registers all types that implement the `IParticipant<T>` interface against the open generic type `IParticipant<>`.
        /// </summary>
        /// <param name="registry">The `IComponentRegistry` instance to register the mapping against.</param>
        /// <param name="assemblyName">The assembly name that contains the types to evaluate.</param>
        public static void RegisterMediatorParticipants(this IComponentRegistry registry, string assemblyName)
        {
            Guard.AgainstNullOrEmptyString(assemblyName, nameof(assemblyName));

            RegisterMediatorParticipants(registry, Assembly.Load(assemblyName));
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

            registry.RegisterCollection(participantType,
                reflectionService.GetTypesAssignableTo(participantType, assembly), Lifestyle.Singleton);
        }
    }
}