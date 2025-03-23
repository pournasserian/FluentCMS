using FluentCMS.Repositories.MongoDB;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

public class MongoDbProviderConfigurator : IProviderConfigurator
{
    public bool CanHandleProvider(string providerName)
    {
        return string.Equals(providerName, "MongoDB", StringComparison.OrdinalIgnoreCase);
    }

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
