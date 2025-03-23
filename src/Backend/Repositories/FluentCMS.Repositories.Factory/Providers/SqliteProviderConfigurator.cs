using FluentCMS.Repositories.EntityFramework;
using FluentCMS.Repositories.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

/// <summary>
/// Configurator for SQLite provider.
/// </summary>
public class SqliteProviderConfigurator : IProviderConfigurator
{
    /// <summary>
    /// Determines if this configurator can handle the specified provider.
    /// </summary>
    /// <param name="providerName">The provider name.</param>
    /// <returns>True if the provider is SQLite; otherwise, false.</returns>
    public bool CanHandleProvider(string providerName)
    {
        return string.Equals(providerName, "SQLite", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Configures SQLite repository services.
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

        // Generate SQLite connection string from options
        string connectionString = $"Data Source={options.SQLite.DatabasePath}";
        
        // Add additional SQLite connection options
        connectionString += $";Foreign Keys={(options.SQLite.EnableForeignKeys ? 1 : 0)}";
        
        if (options.SQLite.UseWal)
        {
            connectionString += ";Journal Mode=WAL";
        }
        
        connectionString += $";Cache Size={options.SQLite.CacheSize}";
        
        if (!options.SQLite.UseConnectionPooling)
        {
            connectionString += ";Pooling=False";
        }
        
        connectionString += $";Default Timeout={options.SQLite.ConnectionTimeout}";

        // Register SQLite repositories with the connection string
        services.AddEntityFrameworkRepositories<SqliteDbContext>(builder =>
        {
            builder.UseSqlite(connectionString);
        });
    }

    /// <summary>
    /// Validates the SQLite configuration.
    /// </summary>
    /// <param name="options">The repository factory options.</param>
    /// <exception cref="ArgumentException">Thrown when SQLite configuration is invalid.</exception>
    public void ValidateConfiguration(RepositoryFactoryOptions options)
    {
        // Check if DatabasePath is provided
        if (string.IsNullOrEmpty(options.SQLite.DatabasePath))
        {
            throw new ArgumentException(
                "SQLite database path must be specified in Repository:SQLite:DatabasePath",
                nameof(options));
        }
    }
}
