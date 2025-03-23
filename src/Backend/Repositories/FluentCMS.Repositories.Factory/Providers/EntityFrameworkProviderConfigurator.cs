using FluentCMS.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

/// <summary>
/// Configurator for Entity Framework Core provider.
/// </summary>
public class EntityFrameworkProviderConfigurator : IProviderConfigurator
{
    /// <summary>
    /// Determines if this configurator can handle the specified provider.
    /// </summary>
    /// <param name="providerName">The provider name.</param>
    /// <returns>True if the provider is EntityFramework; otherwise, false.</returns>
    public bool CanHandleProvider(string providerName)
    {
        return string.Equals(providerName, "EntityFramework", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(providerName, "EF", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Configures Entity Framework repository services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The repository factory options.</param>
    public void ConfigureServices(IServiceCollection services, RepositoryFactoryOptions options)
    {
        // Validate configuration before proceeding
        ValidateConfiguration(options);

        // Configure Entity Framework options
        services.AddEntityFrameworkOptions(efOptions =>
        {
            // Copy options from the EntityFramework section
            efOptions.UseCamelCaseTableNames = options.EntityFramework.UseCamelCaseTableNames;
            efOptions.TableNamePrefix = options.EntityFramework.TableNamePrefix;
            efOptions.TableNameSuffix = options.EntityFramework.TableNameSuffix;
            efOptions.DefaultSchema = options.EntityFramework.DefaultSchema;
            efOptions.AutoMigrateDatabase = options.EntityFramework.AutoMigrateDatabase;
            efOptions.UsePluralTableNames = options.EntityFramework.UsePluralTableNames;
        });

        // For the EntityFramework provider, we only set up options.
        // The actual database context configuration is handled by the specific
        // database providers (SQLite, SqlServer) which will call their respective
        // configuration methods.
    }

    /// <summary>
    /// Validates the Entity Framework configuration.
    /// </summary>
    /// <param name="options">The repository factory options.</param>
    /// <exception cref="ArgumentException">Thrown when Entity Framework configuration is invalid.</exception>
    public void ValidateConfiguration(RepositoryFactoryOptions options)
    {
        // No validation needed for core EF options
        // Connection strings are validated by specific provider configurators
    }
}
