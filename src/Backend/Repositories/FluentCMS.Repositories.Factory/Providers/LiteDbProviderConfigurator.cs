using FluentCMS.Repositories.LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

public class LiteDbProviderConfigurator : IProviderConfigurator
{
    public bool CanHandleProvider(string providerName)
    {
        return string.Equals(providerName, "LiteDB", StringComparison.OrdinalIgnoreCase);
    }

    public void ConfigureServices(IServiceCollection services, RepositoryFactoryOptions options)
    {
        // Validate configuration before proceeding
        ValidateConfiguration(options);

        // Configure LiteDB repositories
        services.AddLiteDb(liteDbOptions =>
        {
            // Copy connection string from the LiteDB section
            liteDbOptions.ConnectionString = options.LiteDB.ConnectionString;
        });
    }

    public void ValidateConfiguration(RepositoryFactoryOptions options)
    {
        if (string.IsNullOrEmpty(options.LiteDB.ConnectionString))
        {
            throw new ArgumentException(
                "LiteDB connection string must be specified in Repository:LiteDB:ConnectionString", 
                nameof(options));
        }
    }
}
