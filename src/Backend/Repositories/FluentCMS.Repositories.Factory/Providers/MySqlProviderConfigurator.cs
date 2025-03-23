using FluentCMS.Repositories.EntityFramework;
using FluentCMS.Repositories.MySQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

public class MySqlProviderConfigurator : IProviderConfigurator
{
    public bool CanHandleProvider(string providerName)
    {
        return string.Equals(providerName, "MySQL", StringComparison.OrdinalIgnoreCase);
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

        // Get server version
        var serverVersion = !string.IsNullOrEmpty(options.MySQL.ServerVersion)
            ? Microsoft.EntityFrameworkCore.ServerVersion.Parse(options.MySQL.ServerVersion)
            : Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(options.MySQL.ConnectionString);

        // Register MySQL repositories
        services.AddEntityFrameworkRepositories<MySqlDbContext>(builder =>
        {
            builder.UseMySql(options.MySQL.ConnectionString, serverVersion, mySqlOptions =>
            {
                mySqlOptions.CommandTimeout(options.MySQL.ConnectionTimeout);
                
                if (options.EntityFramework.AutoMigrateDatabase)
                {
                    mySqlOptions.MigrationsAssembly("FluentCMS.Repositories.MySQL");
                }
            });
        });
    }

    public void ValidateConfiguration(RepositoryFactoryOptions options)
    {
        // Check if connection string is provided
        if (string.IsNullOrEmpty(options.MySQL.ConnectionString))
        {
            throw new ArgumentException(
                "MySQL connection string must be specified in Repository:MySQL:ConnectionString",
                nameof(options));
        }
    }
}
