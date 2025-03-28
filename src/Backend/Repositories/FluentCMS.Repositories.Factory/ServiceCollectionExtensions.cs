using FluentCMS.Repositories.Factory.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositoryFactory(this IServiceCollection services, IConfiguration configuration, string sectionName = "Repository")
    {
        // Configure options from configuration
        services.Configure<RepositoryFactoryOptions>(configuration.GetSection(sectionName));

        // Configure provider-specific options from configuration
        services.Configure<MongoDB.MongoDbOptions>(
            configuration.GetSection($"{sectionName}:MongoDB"));

        services.Configure<LiteDB.LiteDbOptions>(
            configuration.GetSection($"{sectionName}:LiteDB"));

        services.Configure<EntityFramework.EntityFrameworkOptions>(
            configuration.GetSection($"{sectionName}:EntityFramework"));

        services.Configure<SQLite.SqliteOptions>(
            configuration.GetSection($"{sectionName}:SQLite"));

        services.Configure<SqlServer.SqlServerOptions>(
            configuration.GetSection($"{sectionName}:SqlServer"));

        services.Configure<MySQL.MySqlOptions>(
            configuration.GetSection($"{sectionName}:MySQL"));

        services.Configure<PostgreSQL.PostgreSqlOptions>(
            configuration.GetSection($"{sectionName}:PostgreSQL"));

        // Read options to determine which provider to configure
        var options = new RepositoryFactoryOptions();
        configuration.GetSection(sectionName).Bind(options);

        // Configure the appropriate provider
        ConfigureSelectedProvider(services, options);

        // Register the factory as the repository implementation
        services.AddScoped(typeof(Abstractions.IBaseEntityRepository<>),
            typeof(RepositoryFactory<>));

        return services;
    }

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
        services.AddScoped(typeof(Abstractions.IBaseEntityRepository<>),
            typeof(RepositoryFactory<>));

        return services;
    }

    private static void ConfigureSelectedProvider(IServiceCollection services, RepositoryFactoryOptions options)
    {
        // Create all provider configurators
        var configurators = new List<IProviderConfigurator>
        {
            new MongoDbProviderConfigurator(),
            new LiteDbProviderConfigurator(),
            new EntityFrameworkProviderConfigurator(),
            new SqliteProviderConfigurator(),
            new SqlServerProviderConfigurator(),
            new MySqlProviderConfigurator(),
            new PostgreSqlProviderConfigurator()
        };

        // Find the appropriate configurator for the selected provider
        var configurator = configurators.FirstOrDefault(c => c.CanHandleProvider(options.Provider));

        if (configurator == null)
        {
            throw new NotSupportedException(
                $"Repository provider '{options.Provider}' is not supported. " +
                $"Valid providers are: MongoDB, LiteDB, EntityFramework, SQLite, SqlServer, MySQL, PostgreSQL.");
        }

        // Configure the provider
        configurator.ConfigureServices(services, options);
    }
}
