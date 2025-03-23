using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace FluentCMS.Repositories.SQLite;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteRepositories(this IServiceCollection services, Action<SqliteOptions> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register DbContext
        services.AddDbContext<SqliteDbContext>((serviceProvider, options) =>
        {
            var sqliteOptions = new SqliteOptions();
            configure(sqliteOptions);

            // Configure connection string
            var connectionString = BuildConnectionString(sqliteOptions);

            // Configure SQLite
            options.UseSqlite(connectionString, sqliteOptionsBuilder =>
            {
                sqliteOptionsBuilder.CommandTimeout(sqliteOptions.ConnectionTimeout);
            });
        });

        // Register FluentCmsDbContext as SqliteDbContext
        services.AddScoped<FluentCmsDbContext>(sp => sp.GetRequiredService<SqliteDbContext>());

        // Register generic repository
        services.AddScoped(typeof(IBaseEntityRepository<>), typeof(EntityFrameworkEntityRepository<>));

        return services;
    }

    public static IServiceCollection AddSqliteRepositories(this IServiceCollection services, string databasePath, Action<SqliteOptions>? configure = null)
    {
        return services.AddSqliteRepositories(options =>
        {
            options.DatabasePath = databasePath;
            configure?.Invoke(options);
        });
    }

    public static IServiceCollection AddSqliteRepositories(this IServiceCollection services, IConfiguration configuration, string sectionName = "SQLite")
    {
        // Get configuration section
        var section = configuration.GetSection(sectionName);

        return services.AddSqliteRepositories(options =>
        {
            // Read SQLite-specific options
            options.DatabasePath = section["DatabasePath"] ?? options.DatabasePath;

            bool enableForeignKeys;
            if (bool.TryParse(section["EnableForeignKeys"], out enableForeignKeys))
            {
                options.EnableForeignKeys = enableForeignKeys;
            }

            bool useWal;
            if (bool.TryParse(section["UseWal"], out useWal))
            {
                options.UseWal = useWal;
            }

            int cacheSize;
            if (int.TryParse(section["CacheSize"], out cacheSize))
            {
                options.CacheSize = cacheSize;
            }

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

    private static string BuildConnectionString(SqliteOptions options)
    {
        if (string.IsNullOrEmpty(options.DatabasePath))
        {
            throw new ArgumentException("DatabasePath cannot be null or empty", nameof(options));
        }

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(options.DatabasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Create connection string builder
        var connectionStringBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = options.DatabasePath,
            Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate,
            Cache = options.UseConnectionPooling
                ? Microsoft.Data.Sqlite.SqliteCacheMode.Shared
                : Microsoft.Data.Sqlite.SqliteCacheMode.Private,
            ForeignKeys = options.EnableForeignKeys
        };

        // Build the connection string
        var connectionString = connectionStringBuilder.ToString();

        // Add WAL mode if enabled
        if (options.UseWal)
        {
            connectionString += ";Journal Mode=WAL";
        }

        // Add cache size
        connectionString += $";Cache Size={options.CacheSize}";

        return connectionString;
    }
}
