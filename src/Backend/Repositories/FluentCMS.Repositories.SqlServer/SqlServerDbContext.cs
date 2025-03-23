using FluentCMS.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.SqlServer;

public class SqlServerDbContext : FluentCmsDbContext
{
    private readonly SqlServerOptions _sqlServerOptions;

    public SqlServerDbContext(
        DbContextOptions<SqlServerDbContext> options,
        IOptions<SqlServerOptions> sqlServerOptions)
        : base(options, sqlServerOptions)
    {
        _sqlServerOptions = sqlServerOptions?.Value ?? throw new ArgumentNullException(nameof(sqlServerOptions));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Only configure if not already configured
            ConfigureSqlServer(optionsBuilder);
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply SQL Server-specific configurations
        // For example, configuring specific SQL Server data types, constraints, etc.
        
        // Example: Configure default string length for all string properties
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(string) && p.GetMaxLength() == null))
        {
            property.SetMaxLength(256); // Default string length for SQL Server
        }

        base.OnModelCreating(modelBuilder);
    }

    private void ConfigureSqlServer(DbContextOptionsBuilder optionsBuilder)
    {
        if (string.IsNullOrEmpty(_sqlServerOptions.ConnectionString))
        {
            throw new InvalidOperationException("SQL Server connection string is not configured.");
        }

        // Configure SQL Server
        optionsBuilder.UseSqlServer(_sqlServerOptions.ConnectionString, sqlServerOptions =>
        {
            // Configure migrations assembly if needed
            // sqlServerOptions.MigrationsAssembly("FluentCMS.Repositories.SqlServer");

            // Configure command timeout
            sqlServerOptions.CommandTimeout(_sqlServerOptions.CommandTimeout);

            // Configure retry on failure
            if (_sqlServerOptions.EnableRetryOnFailure)
            {
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: _sqlServerOptions.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(_sqlServerOptions.MaxRetryDelay),
                    errorNumbersToAdd: null);
            }

            // Configure MARS
            // This is done at the connection string level - see BuildConnectionString method
        });

        // Configure additional options
        if (_sqlServerOptions.EnableDetailedErrors)
        {
            optionsBuilder.EnableDetailedErrors();
        }

        if (_sqlServerOptions.EnableSensitiveDataLogging)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
