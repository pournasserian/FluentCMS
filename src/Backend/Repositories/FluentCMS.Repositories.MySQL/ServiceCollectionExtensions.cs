using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.MySQL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMySqlRepositories(this IServiceCollection services, Action<MySqlOptions> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register DbContext
        services.AddDbContext<MySqlDbContext>((serviceProvider, options) =>
        {
            var mysqlOptions = new MySqlOptions();
            configure(mysqlOptions);

            // Validate connection string
            if (string.IsNullOrEmpty(mysqlOptions.ConnectionString))
            {
                throw new ArgumentException("MySQL connection string is required", nameof(mysqlOptions));
            }

            // Get server version
            var serverVersion = !string.IsNullOrEmpty(mysqlOptions.ServerVersion)
                ? Microsoft.EntityFrameworkCore.ServerVersion.Parse(mysqlOptions.ServerVersion)
                : Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(mysqlOptions.ConnectionString);

            // Configure MySQL
            options.UseMySql(mysqlOptions.ConnectionString, serverVersion, mysqlOptionsBuilder =>
            {
                mysqlOptionsBuilder.CommandTimeout(mysqlOptions.ConnectionTimeout);

                // Enable auto migrations if configured
                if (mysqlOptions.AutoMigrateDatabase)
                {
                    mysqlOptionsBuilder.MigrationsAssembly("FluentCMS.Repositories.MySQL");
                }
            });
        });

        // Register FluentCmsDbContext as MySqlDbContext
        services.AddScoped<FluentCmsDbContext>(sp => sp.GetRequiredService<MySqlDbContext>());

        // Register generic repository
        services.AddScoped(typeof(IBaseEntityRepository<>), typeof(MySqlEntityRepository<>));

        return services;
    }

    public static IServiceCollection AddMySqlRepositories(this IServiceCollection services,
        string connectionString,
        string? serverVersion = null,
        Action<MySqlOptions>? configure = null)
    {
        return services.AddMySqlRepositories(options =>
        {
            options.ConnectionString = connectionString;

            if (!string.IsNullOrEmpty(serverVersion))
            {
                options.ServerVersion = serverVersion;
            }

            configure?.Invoke(options);
        });
    }

    public static IServiceCollection AddMySqlRepositories(this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "MySQL")
    {
        // Get configuration section
        var section = configuration.GetSection(sectionName);

        return services.AddMySqlRepositories(options =>
        {
            // Read MySQL-specific options
            options.ConnectionString = section["ConnectionString"] ?? options.ConnectionString;
            options.ServerVersion = section["ServerVersion"];
            options.CharacterSet = section["CharacterSet"] ?? options.CharacterSet;

            bool useConnectionPooling;
            if (bool.TryParse(section["UseConnectionPooling"], out useConnectionPooling))
            {
                options.UseConnectionPooling = useConnectionPooling;
            }

            int connectionTimeout;
            if (int.TryParse(section["ConnectionTimeout"], out connectionTimeout))
            {
                options.ConnectionTimeout = connectionTimeout;
            }

            int maxPoolSize;
            if (int.TryParse(section["MaxPoolSize"], out maxPoolSize))
            {
                options.MaxPoolSize = maxPoolSize;
            }

            int minPoolSize;
            if (int.TryParse(section["MinPoolSize"], out minPoolSize))
            {
                options.MinPoolSize = minPoolSize;
            }

            bool useSsl;
            if (bool.TryParse(section["UseSsl"], out useSsl))
            {
                options.UseSsl = useSsl;
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
