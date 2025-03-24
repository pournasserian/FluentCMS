using FluentCMS.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.PostgreSQL;

public class PostgreSqlDbContext : FluentCmsDbContext
{
    private readonly PostgreSqlOptions _postgresOptions;

    public PostgreSqlDbContext(DbContextOptions<PostgreSqlDbContext> options, IOptions<PostgreSqlOptions> postgresOptions) : base(options, postgresOptions)
    {
        _postgresOptions = postgresOptions?.Value ?? throw new ArgumentNullException(nameof(postgresOptions));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Only configure if not already configured
            ConfigurePostgreSql(optionsBuilder);
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply PostgreSQL-specific configurations
        if (!string.IsNullOrEmpty(_postgresOptions.Schema))
        {
            // Set default schema for all entities
            modelBuilder.HasDefaultSchema(_postgresOptions.Schema);
        }

        // Configure JSON column types if enabled
        if (_postgresOptions.UseJsonForComplexTypes)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Look for complex type properties that could benefit from JSON storage
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.ClrType.IsClass &&
                               p.ClrType != typeof(string) &&
                               !p.IsPrimaryKey()))
                {
                    // Configure as JSON column
                    property.SetColumnType("jsonb");
                }
            }
        }

        // Apply case-insensitive collation for text columns if specified
        if (_postgresOptions.UseCaseInsensitiveCollation)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(string)))
                {
                    // Use case-insensitive collation
                    property.SetCollation("und-x-icu");
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private void ConfigurePostgreSql(DbContextOptionsBuilder optionsBuilder)
    {
        // Validate required options
        if (string.IsNullOrEmpty(_postgresOptions.ConnectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is required.");
        }

        // Build connection string with additional options
        var connectionString = BuildConnectionString();

        // Configure PostgreSQL
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            // Configure connection timeout
            npgsqlOptions.CommandTimeout(_postgresOptions.ConnectionTimeout);

            // Enable auto migrations if configured
            if (_postgresOptions.AutoMigrateDatabase)
            {
                npgsqlOptions.MigrationsAssembly("FluentCMS.Repositories.PostgreSQL");
            }

            // Enable retrying on failure
            if (_postgresOptions.EnableRetryOnFailure)
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: _postgresOptions.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }

            // Configure batch operations
            if (_postgresOptions.EnableBatchCommands)
            {
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            }
        });
    }

    private string BuildConnectionString()
    {
        // Start with the base connection string
        var connectionString = _postgresOptions.ConnectionString;

        // If not already in the connection string, add additional options
        if (!connectionString.Contains("Pooling=", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += $";Pooling={(_postgresOptions.UseConnectionPooling ? "true" : "false")}";
        }

        if (_postgresOptions.UseConnectionPooling)
        {
            if (!connectionString.Contains("Maximum Pool Size=", StringComparison.OrdinalIgnoreCase))
            {
                connectionString += $";Maximum Pool Size={_postgresOptions.MaxPoolSize}";
            }

            if (!connectionString.Contains("Minimum Pool Size=", StringComparison.OrdinalIgnoreCase))
            {
                connectionString += $";Minimum Pool Size={_postgresOptions.MinPoolSize}";
            }
        }

        // Add SSL if not already specified
        if (_postgresOptions.UseSsl && !connectionString.Contains("SSL Mode=", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += $";SSL Mode={_postgresOptions.SslMode}";
        }

        return connectionString;
    }
}
