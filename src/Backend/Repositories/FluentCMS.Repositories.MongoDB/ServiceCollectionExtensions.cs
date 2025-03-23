using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.MongoDB;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDbRepositories(this IServiceCollection services, Action<MongoDbOptions> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register generic repository
        services.AddScoped(typeof(IBaseEntityRepository<>), typeof(MongoDbEntityRepository<>));

        return services;
    }

    public static IServiceCollection AddMongoDbRepositories(this IServiceCollection services, IConfiguration configuration, string sectionName = "MongoDB")
    {
        // Bind configuration to options
        services.Configure<MongoDbOptions>(configuration.GetSection(sectionName));

        // Register generic repository
        services.AddScoped(typeof(IBaseEntityRepository<>), typeof(MongoDbEntityRepository<>));

        return services;
    }

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

    public static IServiceCollection AddMongoDbRepository<TEntity>(this IServiceCollection services) where TEntity : class, IBaseEntity
    {
        services.AddScoped<IBaseEntityRepository<TEntity>, MongoDbEntityRepository<TEntity>>();
        return services;
    }

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
