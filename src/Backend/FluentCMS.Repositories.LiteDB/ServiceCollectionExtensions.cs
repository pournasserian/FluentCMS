using FluentCMS.Repositories.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentCMS.Repositories.LiteDB;

/// <summary>
/// Extension methods for setting up LiteDB repositories in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds LiteDB repository services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configuration">The configuration being bound.</param>
    /// <param name="configSection">The configuration section name. Default is "LiteDb".</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddLiteDb(this IServiceCollection services, IConfiguration configuration, string configSection = "LiteDb")
    {
        // Configure LiteDB options from the specified section
        services.Configure<LiteDbOptions>(configuration.GetSection(configSection));

        // Register the generic repository
        services.TryAddScoped(typeof(IBaseEntityRepository<>), typeof(LiteDbEntityRepository<>));

        return services;
    }

    /// <summary>
    /// Adds LiteDB repository services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configure">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddLiteDb(this IServiceCollection services, Action<LiteDbOptions> configure)
    {
        // Configure LiteDB options using the provided action
        services.Configure(configure);

        // Register the generic repository
        services.TryAddScoped(typeof(IBaseEntityRepository<>), typeof(LiteDbEntityRepository<>));

        return services;
    }
}
