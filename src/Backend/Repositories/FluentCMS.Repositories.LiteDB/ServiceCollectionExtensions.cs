using FluentCMS.Repositories.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentCMS.Repositories.LiteDB;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLiteDb(this IServiceCollection services, IConfiguration configuration, string configSection = "LiteDb")
    {
        // Configure LiteDB options from the specified section
        services.Configure<LiteDbOptions>(configuration.GetSection(configSection));

        // Register the generic repository
        services.TryAddScoped(typeof(IBaseEntityRepository<>), typeof(LiteDbEntityRepository<>));

        return services;
    }

    public static IServiceCollection AddLiteDb(this IServiceCollection services, Action<LiteDbOptions> configure)
    {
        // Configure LiteDB options using the provided action
        services.Configure(configure);

        // Register the generic repository
        services.TryAddScoped(typeof(IBaseEntityRepository<>), typeof(LiteDbEntityRepository<>));

        return services;
    }
}
