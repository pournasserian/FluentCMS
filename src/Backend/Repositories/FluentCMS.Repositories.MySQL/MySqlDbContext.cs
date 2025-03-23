using FluentCMS.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace FluentCMS.Repositories.MySQL;

/// <summary>
/// MySQL implementation of the FluentCMS DbContext.
/// </summary>
public class MySqlDbContext : FluentCmsDbContext
{
    private readonly MySqlOptions _mysqlOptions;

    /// <summary>
    /// Initializes a new instance of the MySqlDbContext class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    /// <param name="mysqlOptions">The MySQL configuration options.</param>
    public MySqlDbContext(
        DbContextOptions<MySqlDbContext> options,
        IOptions<MySqlOptions> mysqlOptions)
        : base(options, mysqlOptions)
    {
        _mysqlOptions = mysqlOptions?.Value ?? throw new ArgumentNullException(nameof(mysqlOptions));
    }

    /// <summary>
    /// Configures MySQL-specific options when the context is being configured.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Only configure if not already configured
            ConfigureMySql(optionsBuilder);
        }

        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    /// Configures MySQL-specific model creation options.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply MySQL-specific configurations
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // MySQL has a default limit on key lengths for strings, ensure string primary keys have appropriate length
            foreach (var property in entityType.GetProperties()
                .Where(p => p.IsKey() && (p.ClrType == typeof(string))))
            {
                property.SetMaxLength(255);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Applies MySQL-specific configuration to the DbContext options.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    private void ConfigureMySql(DbContextOptionsBuilder optionsBuilder)
    {
        // Validate required options
        if (string.IsNullOrEmpty(_mysqlOptions.ConnectionString))
        {
            throw new InvalidOperationException("MySQL connection string is required.");
        }

        // Build connection string with additional options
        var connectionString = BuildConnectionString();

        // Get the server version
        var serverVersion = GetServerVersion();

        // Configure MySQL
        optionsBuilder.UseMySql(connectionString, serverVersion, mysqlOptions =>
        {
            // Configure connection timeout
            mysqlOptions.CommandTimeout(_mysqlOptions.ConnectionTimeout);
            
            // Enable auto migrations if configured
            if (_mysqlOptions.AutoMigrateDatabase)
            {
                mysqlOptions.MigrationsAssembly("FluentCMS.Repositories.MySQL");
            }
        });
    }

    /// <summary>
    /// Builds the MySQL connection string based on the options.
    /// </summary>
    /// <returns>The connection string.</returns>
    private string BuildConnectionString()
    {
        // Start with the base connection string
        var connectionString = _mysqlOptions.ConnectionString;
        
        // If not already in the connection string, add additional options
        if (!connectionString.Contains("Pooling=", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += $";Pooling={(_mysqlOptions.UseConnectionPooling ? "true" : "false")}";
        }
        
        if (_mysqlOptions.UseConnectionPooling)
        {
            if (!connectionString.Contains("Maximum Pool Size=", StringComparison.OrdinalIgnoreCase))
            {
                connectionString += $";Maximum Pool Size={_mysqlOptions.MaxPoolSize}";
            }
            
            if (!connectionString.Contains("Minimum Pool Size=", StringComparison.OrdinalIgnoreCase))
            {
                connectionString += $";Minimum Pool Size={_mysqlOptions.MinPoolSize}";
            }
        }
        
        // Add SSL if not already specified
        if (_mysqlOptions.UseSsl && !connectionString.Contains("SslMode=", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += ";SslMode=Required";
        }

        return connectionString;
    }

    /// <summary>
    /// Gets the MySQL server version from configuration.
    /// </summary>
    /// <returns>The server version.</returns>
    private Microsoft.EntityFrameworkCore.ServerVersion GetServerVersion()
    {
        // If specific server version is provided in options, use it
        if (!string.IsNullOrEmpty(_mysqlOptions.ServerVersion))
        {
            return Microsoft.EntityFrameworkCore.ServerVersion.Parse(_mysqlOptions.ServerVersion);
        }
        
        // Otherwise auto-detect from connection string
        return Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(_mysqlOptions.ConnectionString);
    }
}
