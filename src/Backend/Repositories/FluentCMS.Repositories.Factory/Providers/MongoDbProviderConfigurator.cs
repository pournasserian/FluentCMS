using FluentCMS.Repositories.MongoDB;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

/// <summary>
/// Configurator for MongoDB provider.
/// </summary>
public class MongoDbProviderConfigurator : IProviderConfigurator
{
    /// <summary>
    /// Determines if this configurator can handle the specified provider.
    /// </summary>
    /// <param name="providerName">The provider name.</param>
    /// <returns>True if the provider is MongoDB; otherwise, false.</returns>
    public bool CanHandleProvider(string providerName)
    {
        return string.Equals(providerName, "MongoDB", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Configures MongoDB repository services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The repository factory options.</param>
    public void ConfigureServices(IServiceCollection services, RepositoryFactoryOptions options)
    {
        // Validate configuration before proceeding
        ValidateConfiguration(options);

        // Configure MongoDB repositories
        services.AddMongoDbRepositories(mongoOptions =>
        {
            // Copy options from the MongoDB section
            mongoOptions.ConnectionString = options.MongoDB.ConnectionString;
            mongoOptions.DatabaseName = options.MongoDB.DatabaseName;
            mongoOptions.UseCamelCaseCollectionNames = options.MongoDB.UseCamelCaseCollectionNames;
            mongoOptions.CollectionNamePrefix = options.MongoDB.CollectionNamePrefix;
            mongoOptions.CollectionNameSuffix = options.MongoDB.CollectionNameSuffix;
        });
    }

    /// <summary>
    /// Validates the MongoDB configuration.
    /// </summary>
    /// <param name="options">The repository factory options.</param>
    /// <exception cref="ArgumentException">Thrown when MongoDB configuration is invalid.</exception>
    public void ValidateConfiguration(RepositoryFactoryOptions options)
    {
        if (string.IsNullOrEmpty(options.MongoDB.ConnectionString))
        {
            throw new ArgumentException(
                "MongoDB connection string must be specified in Repository:MongoDB:ConnectionString", 
                nameof(options));
        }

        if (string.IsNullOrEmpty(options.MongoDB.DatabaseName))
        {
            throw new ArgumentException(
                "MongoDB database name must be specified in Repository:MongoDB:DatabaseName", 
                nameof(options));
        }
    }
}
