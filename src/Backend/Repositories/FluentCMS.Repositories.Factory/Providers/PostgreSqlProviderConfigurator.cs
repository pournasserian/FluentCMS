using FluentCMS.Repositories.EntityFramework;
using FluentCMS.Repositories.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

public class PostgreSqlProviderConfigurator : IProviderConfigurator
{
    public bool CanHandleProvider(string providerName)
    {
        return providerName.Equals("postgresql", StringComparison.OrdinalIgnoreCase) || 
               providerName.Equals("postgres", StringComparison.OrdinalIgnoreCase) ||
               providerName.Equals("npgsql", StringComparison.OrdinalIgnoreCase);
    }

    public void ConfigureServices(IServiceCollection services, RepositoryFactoryOptions options)
    {
        // Validate configuration
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

        // Register PostgreSQL repositories
        services.AddEntityFrameworkRepositories<PostgreSqlDbContext>(builder =>
        {
            builder.UseNpgsql(options.PostgreSQL.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(options.PostgreSQL.ConnectionTimeout);
                
                if (options.EntityFramework.AutoMigrateDatabase)
                {
                    npgsqlOptions.MigrationsAssembly("FluentCMS.Repositories.PostgreSQL");
                }
                
                if (options.PostgreSQL.EnableRetryOnFailure)
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: options.PostgreSQL.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                }
                
                if (options.PostgreSQL.EnableBatchCommands)
                {
                    npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }
            });
        });
    }

    public void ValidateConfiguration(RepositoryFactoryOptions options)
    {
        // Check if connection string is provided
        if (string.IsNullOrEmpty(options.PostgreSQL.ConnectionString))
        {
            throw new ArgumentException(
                "PostgreSQL connection string must be specified in Repository:PostgreSQL:ConnectionString",
                nameof(options));
        }
        
        // Validate that the connection string is a valid PostgreSQL connection string
        var connectionString = options.PostgreSQL.ConnectionString;
        if (!connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid PostgreSQL connection string. It should include Host or Server.", nameof(options));
        }
    }
}
