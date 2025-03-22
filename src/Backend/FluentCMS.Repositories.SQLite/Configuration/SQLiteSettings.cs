using System;

namespace FluentCMS.Repositories.SQLite.Configuration;

/// <summary>
/// Settings for SQLite connection and configuration.
/// </summary>
public class SQLiteSettings
{
    /// <summary>
    /// Gets or sets the connection string for SQLite.
    /// This is typically the path to the database file.
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
    /// Gets or sets the table prefix for all tables in the database.
    /// </summary>
    public string? TablePrefix { get; set; }
    
    /// <summary>
    /// Gets or sets the timeout for database operations in seconds. Default is 30 seconds.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
    
    /// <summary>
    /// Gets or sets the maximum retry count for transient failures. Default is 0 (no retry).
    /// </summary>
    public int MaxRetryCount { get; set; } = 0;
    
    /// <summary>
    /// Validates the settings.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when required settings are not provided.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new ArgumentException("SQLite connection string is required.", nameof(ConnectionString));
    }
}
