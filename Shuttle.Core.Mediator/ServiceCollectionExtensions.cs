using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            builder?.Invoke(new MediatorBuilder(services));

            services.TryAddSingleton<IMediator, Mediator>();

            return services;
        }
    }
}