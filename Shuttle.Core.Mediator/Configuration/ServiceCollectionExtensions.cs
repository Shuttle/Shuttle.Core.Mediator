using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorOptions> options = null)
        {
            Guard.AgainstNull(services, nameof(services));

            options?.Invoke(new MediatorOptions(services));

            services.TryAddSingleton<IMediator, Mediator>();

            return services;
        }
    }
}