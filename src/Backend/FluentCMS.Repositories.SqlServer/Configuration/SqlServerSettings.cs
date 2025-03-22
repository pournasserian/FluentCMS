using System;

namespace FluentCMS.Repositories.SqlServer.Configuration;

/// <summary>
/// Settings for SQL Server connection and configuration.
/// </summary>
public class SqlServerSettings
{
    /// <summary>
    /// Gets or sets the connection string for SQL Server.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether to apply a global soft-delete filter.
    /// </summary>
    public bool ApplySoftDeleteFilter { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether to use a shared database context or create a new one per scope.
    /// </summary>
    public bool UseSharedContext { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether the database should be migrated automatically.
    /// </summary>
    public bool AutoMigrate { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether the database context should auto-detect changes.
    /// </summary>
    public bool AutoDetectChanges { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether to use detailed errors in the database context.
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether to enable sensitive data logging.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the schema name for all tables in the database.
    /// </summary>
    public string? SchemaName { get; set; } = "dbo";
    
    /// <summary>
    /// Gets or sets the table prefix for all tables in the database.
    /// </summary>
    public string? TablePrefix { get; set; }
    
    /// <summary>
    /// Gets or sets the timeout for database operations in seconds. Default is 30 seconds.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
    
    /// <summary>
    /// Gets or sets the maximum retry count for transient failures. Default is 3.
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;
    
    /// <summary>
    /// Gets or sets the maximum retry delay in seconds. Default is 30 seconds.
    /// </summary>
    public int MaxRetryDelay { get; set; } = 30;
    
    /// <summary>
    /// Gets or sets a value indicating whether to use table partitioning for large tables.
    /// Default is false.
    /// </summary>
    public bool UseTablePartitioning { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether to use query hints to optimize performance.
    /// Default is false.
    /// </summary>
    public bool UseQueryHints { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether to enable SQL Server specific features like table-valued parameters.
    /// Default is true.
    /// </summary>
    public bool EnableSqlServerFeatures { get; set; } = true;
    
    /// <summary>
    /// Validates the settings.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when required settings are not provided.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new ArgumentException("SQL Server connection string is required.", nameof(ConnectionString));
    }
}
