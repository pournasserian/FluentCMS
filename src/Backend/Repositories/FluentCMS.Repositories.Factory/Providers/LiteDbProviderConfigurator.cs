using FluentCMS.Repositories.LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

/// <summary>
/// Configurator for LiteDB provider.
/// </summary>
public class LiteDbProviderConfigurator : IProviderConfigurator
{
    /// <summary>
    /// Determines if this configurator can handle the specified provider.
    /// </summary>
    /// <param name="providerName">The provider name.</param>
    /// <returns>True if the provider is LiteDB; otherwise, false.</returns>
    public bool CanHandleProvider(string providerName)
    {
        return string.Equals(providerName, "LiteDB", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Configures LiteDB repository services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The repository factory options.</param>
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

    /// <summary>
    /// Validates the LiteDB configuration.
    /// </summary>
    /// <param name="options">The repository factory options.</param>
    /// <exception cref="ArgumentException">Thrown when LiteDB configuration is invalid.</exception>
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
