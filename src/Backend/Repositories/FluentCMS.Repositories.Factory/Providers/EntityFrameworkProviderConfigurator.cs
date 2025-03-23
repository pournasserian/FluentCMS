using FluentCMS.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

public class EntityFrameworkProviderConfigurator : IProviderConfigurator
{
    public bool CanHandleProvider(string providerName)
    {
        return string.Equals(providerName, "EntityFramework", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(providerName, "EF", StringComparison.OrdinalIgnoreCase);
    }

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

    public void ValidateConfiguration(RepositoryFactoryOptions options)
    {
        // No validation needed for core EF options
        // Connection strings are validated by specific provider configurators
    }
}
