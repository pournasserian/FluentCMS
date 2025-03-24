using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.PostgreSQL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgreSqlRepositories(this IServiceCollection services, Action<PostgreSqlOptions> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register DbContext
        services.AddDbContext<PostgreSqlDbContext>((serviceProvider, options) =>
        {
            var postgresOptions = new PostgreSqlOptions();
            configure(postgresOptions);

            // Configure connection string
            var connectionString = postgresOptions.ConnectionString;

            // Configure PostgreSQL
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(postgresOptions.ConnectionTimeout);

                if (postgresOptions.EnableRetryOnFailure)
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: postgresOptions.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                }

                if (postgresOptions.EnableBatchCommands)
                {
                    npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }

                if (postgresOptions.AutoMigrateDatabase)
                {
                    npgsqlOptions.MigrationsAssembly("FluentCMS.Repositories.PostgreSQL");
                }
            });
        });

        // Register FluentCmsDbContext as PostgreSqlDbContext
        services.AddScoped<FluentCmsDbContext>(sp => sp.GetRequiredService<PostgreSqlDbContext>());

        // Register generic repository
        services.AddScoped(typeof(IBaseEntityRepository<>), typeof(PostgreSqlEntityRepository<>));

        return services;
    }

    public static IServiceCollection AddPostgreSqlRepositories(this IServiceCollection services, string connectionString, Action<PostgreSqlOptions>? configure = null)
    {
        return services.AddPostgreSqlRepositories(options =>
        {
            options.ConnectionString = connectionString;
            configure?.Invoke(options);
        });
    }

    public static IServiceCollection AddPostgreSqlRepositories(this IServiceCollection services, IConfiguration configuration, string sectionName = "PostgreSQL")
    {
        // Get configuration section
        var section = configuration.GetSection(sectionName);

        return services.AddPostgreSqlRepositories(options =>
        {
            // Read PostgreSQL-specific options
            options.ConnectionString = section["ConnectionString"] ?? options.ConnectionString;
            options.ServerVersion = section["ServerVersion"];

            bool useJsonForComplexTypes;
            if (bool.TryParse(section["UseJsonForComplexTypes"], out useJsonForComplexTypes))
            {
                options.UseJsonForComplexTypes = useJsonForComplexTypes;
            }

            bool useCaseInsensitiveCollation;
            if (bool.TryParse(section["UseCaseInsensitiveCollation"], out useCaseInsensitiveCollation))
            {
                options.UseCaseInsensitiveCollation = useCaseInsensitiveCollation;
            }

            options.Schema = section["Schema"];

            bool useSsl;
            if (bool.TryParse(section["UseSsl"], out useSsl))
            {
                options.UseSsl = useSsl;
            }

            options.SslMode = section["SslMode"] ?? options.SslMode;

            bool useConnectionPooling;
            if (bool.TryParse(section["UseConnectionPooling"], out useConnectionPooling))
            {
                options.UseConnectionPooling = useConnectionPooling;
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

            int connectionTimeout;
            if (int.TryParse(section["ConnectionTimeout"], out connectionTimeout))
            {
                options.ConnectionTimeout = connectionTimeout;
            }

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

            bool enableBatchCommands;
            if (bool.TryParse(section["EnableBatchCommands"], out enableBatchCommands))
            {
                options.EnableBatchCommands = enableBatchCommands;
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
