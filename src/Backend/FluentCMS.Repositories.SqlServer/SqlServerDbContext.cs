using FluentCMS.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.SqlServer;

/// <summary>
/// SQL Server implementation of the FluentCMS DbContext.
/// </summary>
public class SqlServerDbContext : FluentCmsDbContext
{
    private readonly SqlServerOptions _sqlServerOptions;

    /// <summary>
    /// Initializes a new instance of the SqlServerDbContext class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    /// <param name="sqlServerOptions">The SQL Server configuration options.</param>
    public SqlServerDbContext(
        DbContextOptions<SqlServerDbContext> options,
        IOptions<SqlServerOptions> sqlServerOptions)
        : base(options, sqlServerOptions)
    {
        _sqlServerOptions = sqlServerOptions?.Value ?? throw new ArgumentNullException(nameof(sqlServerOptions));
    }

    /// <summary>
    /// Configures SQL Server-specific options when the context is being configured.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Only configure if not already configured
            ConfigureSqlServer(optionsBuilder);
        }

        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    /// Configures SQL Server-specific model creation options.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
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

    /// <summary>
    /// Applies SQL Server-specific configuration to the DbContext options.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
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
