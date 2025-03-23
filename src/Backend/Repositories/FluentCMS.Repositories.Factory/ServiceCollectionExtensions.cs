using FluentCMS.Repositories.Factory.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory;

/// <summary>
/// Extension methods for setting up the repository factory in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the repository factory to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configuration">The configuration being bound.</param>
    /// <param name="sectionName">The configuration section name. Default is "Repository".</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddRepositoryFactory(
        this IServiceCollection services, 
        IConfiguration configuration, 
        string sectionName = "Repository")
    {
        // Configure options from configuration
        services.Configure<RepositoryFactoryOptions>(configuration.GetSection(sectionName));
        
        // Configure provider-specific options from configuration
        services.Configure<FluentCMS.Repositories.MongoDB.MongoDbOptions>(
            configuration.GetSection($"{sectionName}:MongoDB"));
        
        services.Configure<FluentCMS.Repositories.LiteDB.LiteDbOptions>(
            configuration.GetSection($"{sectionName}:LiteDB"));
        
        services.Configure<FluentCMS.Repositories.EntityFramework.EntityFrameworkOptions>(
            configuration.GetSection($"{sectionName}:EntityFramework"));
        
        services.Configure<FluentCMS.Repositories.SQLite.SqliteOptions>(
            configuration.GetSection($"{sectionName}:SQLite"));
        
        services.Configure<FluentCMS.Repositories.SqlServer.SqlServerOptions>(
            configuration.GetSection($"{sectionName}:SqlServer"));

        // Read options to determine which provider to configure
        var options = new RepositoryFactoryOptions();
        configuration.GetSection(sectionName).Bind(options);

        // Configure the appropriate provider
        ConfigureSelectedProvider(services, options);
        
        // Register the factory as the repository implementation
        services.AddScoped(typeof(FluentCMS.Repositories.Abstractions.IBaseEntityRepository<>), 
            typeof(RepositoryFactory<>));
        
        return services;
    }

    /// <summary>
    /// Adds the repository factory to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configure">The action used to configure the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddRepositoryFactory(
        this IServiceCollection services,
        Action<RepositoryFactoryOptions> configure)
    {
        // Configure options using the provided action
        services.Configure(configure);
        
        // Create options instance to determine which provider to configure
        var options = new RepositoryFactoryOptions();
        configure(options);
        
        // Configure the appropriate provider
        ConfigureSelectedProvider(services, options);
        
        // Register the factory as the repository implementation
        services.AddScoped(typeof(FluentCMS.Repositories.Abstractions.IBaseEntityRepository<>), 
            typeof(RepositoryFactory<>));
        
        return services;
    }

    /// <summary>
    /// Configures the selected provider based on the options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The repository factory options.</param>
    private static void ConfigureSelectedProvider(IServiceCollection services, RepositoryFactoryOptions options)
    {
        // Create all provider configurators
        var configurators = new List<IProviderConfigurator>
        {
            new MongoDbProviderConfigurator(),
            new LiteDbProviderConfigurator(),
            new EntityFrameworkProviderConfigurator(),
            new SqliteProviderConfigurator(),
            new SqlServerProviderConfigurator()
        };
        
        // Find the appropriate configurator for the selected provider
        var configurator = configurators.FirstOrDefault(c => c.CanHandleProvider(options.Provider));
        
        if (configurator == null)
        {
            throw new NotSupportedException(
                $"Repository provider '{options.Provider}' is not supported. " +
                $"Valid providers are: MongoDB, LiteDB, EntityFramework, SQLite, SqlServer.");
        }
        
        // Configure the provider
        configurator.ConfigureServices(services, options);
    }
}
