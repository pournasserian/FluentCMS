using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.EntityFramework;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.SqlServer;

/// <summary>
/// Extension methods for configuring SQL Server repositories with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQL Server repository services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure the SQL Server options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddSqlServerRepositories(this IServiceCollection services, Action<SqlServerOptions> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register DbContext
        services.AddDbContext<SqlServerDbContext>((serviceProvider, options) =>
        {
            var sqlServerOptions = new SqlServerOptions();
            configure(sqlServerOptions);

            // Ensure connection string is configured
            if (string.IsNullOrEmpty(sqlServerOptions.ConnectionString))
            {
                throw new InvalidOperationException("SQL Server connection string is not configured.");
            }

            // Apply MARS setting if needed
            var connectionString = sqlServerOptions.ConnectionString;
            if (sqlServerOptions.EnableMultipleActiveResultSets &&
                !connectionString.Contains("MultipleActiveResultSets=true", StringComparison.OrdinalIgnoreCase))
            {
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    MultipleActiveResultSets = true
                };
                connectionString = builder.ConnectionString;
            }

            // Configure SQL Server
            options.UseSqlServer(connectionString, sqlServerOptionsBuilder =>
            {
                // Configure command timeout
                sqlServerOptionsBuilder.CommandTimeout(sqlServerOptions.CommandTimeout);

                // Configure retry on failure
                if (sqlServerOptions.EnableRetryOnFailure)
                {
                    sqlServerOptionsBuilder.EnableRetryOnFailure(
                        maxRetryCount: sqlServerOptions.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(sqlServerOptions.MaxRetryDelay),
                        errorNumbersToAdd: null);
                }
            });

            // Configure additional options
            if (sqlServerOptions.EnableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }

            if (sqlServerOptions.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Register FluentCmsDbContext as SqlServerDbContext
        services.AddScoped<FluentCmsDbContext>(sp => sp.GetRequiredService<SqlServerDbContext>());

        // Register generic repository
        services.AddScoped(typeof(IBaseEntityRepository<>), typeof(EntityFrameworkEntityRepository<>));

        return services;
    }

    /// <summary>
    /// Adds SQL Server repository services to the service collection using a connection string.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="configure">An optional action to configure additional SQL Server options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddSqlServerRepositories(this IServiceCollection services, string connectionString, Action<SqlServerOptions>? configure = null)
    {
        return services.AddSqlServerRepositories(options =>
        {
            options.ConnectionString = connectionString;
            configure?.Invoke(options);
        });
    }

    /// <summary>
    /// Adds SQL Server repository services to the service collection using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section containing SQL Server settings.</param>
    /// <param name="sectionName">The name of the configuration section to use. Defaults to "SqlServer".</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddSqlServerRepositories(this IServiceCollection services, IConfiguration configuration, string sectionName = "SqlServer")
    {
        // Get configuration section
        var section = configuration.GetSection(sectionName);

        return services.AddSqlServerRepositories(options =>
        {
            // Read SQL Server-specific options
            options.ConnectionString = section["ConnectionString"] ?? options.ConnectionString;

            bool enableRetryOnFailure;
            if (bool.TryParse(section["EnableRetryOnFailure"], out enableRetryOnFailure))
            {
                options.EnableRetryOnFailure = enableRetryOnFailure;
            }

            int maxRetryCount;
            if (int.TryParse(section["MaxRetryCount"], out maxRetryCount))
            {
                options.MaxRetryCount = maxRetryCount;
            }

            int maxRetryDelay;
            if (int.TryParse(section["MaxRetryDelay"], out maxRetryDelay))
            {
                options.MaxRetryDelay = maxRetryDelay;
            }

            int commandTimeout;
            if (int.TryParse(section["CommandTimeout"], out commandTimeout))
            {
                options.CommandTimeout = commandTimeout;
            }

            bool enableSensitiveDataLogging;
            if (bool.TryParse(section["EnableSensitiveDataLogging"], out enableSensitiveDataLogging))
            {
                options.EnableSensitiveDataLogging = enableSensitiveDataLogging;
            }

            bool enableDetailedErrors;
            if (bool.TryParse(section["EnableDetailedErrors"], out enableDetailedErrors))
            {
                options.EnableDetailedErrors = enableDetailedErrors;
            }

            bool enableMultipleActiveResultSets;
            if (bool.TryParse(section["EnableMultipleActiveResultSets"], out enableMultipleActiveResultSets))
            {
                options.EnableMultipleActiveResultSets = enableMultipleActiveResultSets;
            }

            // Read base options
            bool useCamelCaseTableNames;
            if (bool.TryParse(section["UseCamelCaseTableNames"], out useCamelCaseTableNames))
            {
                options.UseCamelCaseTableNames = useCamelCaseTableNames;
            }

            options.TableNamePrefix = section["TableNamePrefix"];
            options.TableNameSuffix = section["TableNameSuffix"];
            options.DefaultSchema = section["DefaultSchema"];

            bool autoMigrateDatabase;
            if (bool.TryParse(section["AutoMigrateDatabase"], out autoMigrateDatabase))
            {
                options.AutoMigrateDatabase = autoMigrateDatabase;
            }

            bool usePluralTableNames;
            if (bool.TryParse(section["UsePluralTableNames"], out usePluralTableNames))
            {
                options.UsePluralTableNames = usePluralTableNames;
            }
        });
    }
}
