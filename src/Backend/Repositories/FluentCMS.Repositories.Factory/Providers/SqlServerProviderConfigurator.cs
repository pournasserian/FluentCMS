using FluentCMS.Repositories.EntityFramework;
using FluentCMS.Repositories.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

/// <summary>
/// Configurator for SQL Server provider.
/// </summary>
public class SqlServerProviderConfigurator : IProviderConfigurator
{
    /// <summary>
    /// Determines if this configurator can handle the specified provider.
    /// </summary>
    /// <param name="providerName">The provider name.</param>
    /// <returns>True if the provider is SqlServer; otherwise, false.</returns>
    public bool CanHandleProvider(string providerName)
    {
        return string.Equals(providerName, "SqlServer", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(providerName, "SQL Server", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(providerName, "MSSQL", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Configures SQL Server repository services.
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

        // Get the connection string
        string connectionString = options.SqlServer.ConnectionString;

        // Register SQL Server repositories with the connection string
        services.AddEntityFrameworkRepositories<SqlServerDbContext>(builder =>
        {
            builder.UseSqlServer(connectionString, sqlOptions =>
            {
                // Apply SQL Server specific options if provided
                if (options.SqlServer.CommandTimeout > 0)
                {
                    sqlOptions.CommandTimeout(options.SqlServer.CommandTimeout);
                }
                
                if (options.SqlServer.EnableRetryOnFailure)
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: options.SqlServer.MaxRetryCount > 0 
                            ? options.SqlServer.MaxRetryCount 
                            : 5);
                }
                
                // Apply other SQL Server specific options as needed
                if (options.SqlServer.EnableSensitiveDataLogging)
                {
                    builder.EnableSensitiveDataLogging();
                }
            });
        });
    }

    /// <summary>
    /// Validates the SQL Server configuration.
    /// </summary>
    /// <param name="options">The repository factory options.</param>
    /// <exception cref="ArgumentException">Thrown when SQL Server configuration is invalid.</exception>
    public void ValidateConfiguration(RepositoryFactoryOptions options)
    {
        // Check if connection string is provided
        if (string.IsNullOrEmpty(options.SqlServer.ConnectionString))
        {
            throw new ArgumentException(
                "SQL Server connection string must be specified in Repository:SqlServer:ConnectionString",
                nameof(options));
        }
    }
}
