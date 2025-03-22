using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.MongoDB;

/// <summary>
/// Extension methods for configuring MongoDB repositories with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MongoDB repository services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure the MongoDB options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddMongoDbRepositories(this IServiceCollection services, Action<MongoDbOptions> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register generic repository
        services.AddScoped(typeof(IBaseEntityRepository<>), typeof(MongoDbEntityRepository<>));

        return services;
    }

    /// <summary>
    /// Adds MongoDB repository services to the service collection using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section containing MongoDB settings.</param>
    /// <param name="sectionName">The name of the configuration section to use. Defaults to "MongoDB".</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddMongoDbRepositories(this IServiceCollection services, IConfiguration configuration, string sectionName = "MongoDB")
    {
        // Bind configuration to options
        services.Configure<MongoDbOptions>(configuration.GetSection(sectionName));

        // Register generic repository
        services.AddScoped(typeof(IBaseEntityRepository<>), typeof(MongoDbEntityRepository<>));

        return services;
    }

    /// <summary>
    /// Adds MongoDB repository services to the service collection with connection string and database name.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The MongoDB connection string.</param>
    /// <param name="databaseName">The MongoDB database name.</param>
    /// <param name="configure">An optional action to configure additional MongoDB options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddMongoDbRepositories(this IServiceCollection services, string connectionString, string databaseName, Action<MongoDbOptions>? configure = null)
    {
        return services.AddMongoDbRepositories(options =>
        {
            options.ConnectionString = connectionString;
            options.DatabaseName = databaseName;

            // Apply additional configuration if provided
            configure?.Invoke(options);
        });
    }

    /// <summary>
    /// Registers a specific repository implementation for an entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddMongoDbRepository<TEntity>(this IServiceCollection services) where TEntity : class, IBaseEntity
    {
        services.AddScoped<IBaseEntityRepository<TEntity>, MongoDbEntityRepository<TEntity>>();
        return services;
    }

    /// <summary>
    /// Creates a MongoDB connection string from components.
    /// </summary>
    /// <param name="host">The MongoDB server host.</param>
    /// <param name="port">The MongoDB server port.</param>
    /// <param name="username">The username for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    /// <param name="authSource">The authentication source database. Defaults to "admin".</param>
    /// <param name="useSsl">Whether to use SSL for the connection. Defaults to true.</param>
    /// <returns>A MongoDB connection string.</returns>
    public static string CreateMongoConnectionString(string host, int port, string? username = null, string? password = null, string authSource = "admin", bool useSsl = true)
    {
        if (string.IsNullOrEmpty(host))
        {
            throw new ArgumentException("Host cannot be null or empty", nameof(host));
        }

        if (port <= 0)
        {
            throw new ArgumentException("Port must be greater than zero", nameof(port));
        }

        // Build the connection string
        var connectionString = $"mongodb://";

        // Add credentials if provided
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            connectionString += $"{Uri.EscapeDataString(username)}:{Uri.EscapeDataString(password)}@";
        }

        // Add server and port
        connectionString += $"{host}:{port}";

        // Add connection parameters
        connectionString += "/?";

        // Add auth source if credentials were provided
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            connectionString += $"authSource={authSource}&";
        }

        // Add SSL setting
        connectionString += $"ssl={useSsl.ToString().ToLowerInvariant()}";

        return connectionString;
    }
}
