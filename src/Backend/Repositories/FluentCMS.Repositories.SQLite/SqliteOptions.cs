namespace FluentCMS.Repositories.SQLite;

using FluentCMS.Repositories.EntityFramework;

/// <summary>
/// Configuration options for SQLite repositories.
/// </summary>
public class SqliteOptions : EntityFrameworkOptions
{
    /// <summary>
    /// Gets or sets the SQLite database file path.
    /// </summary>
    public string DatabasePath { get; set; } = "fluentcms.db";
    
    /// <summary>
    /// Gets or sets whether to enable foreign key constraints.
    /// Default is true.
    /// </summary>
    public bool EnableForeignKeys { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use WAL (Write-Ahead Logging) journal mode.
    /// Default is true. WAL typically improves performance.
    /// </summary>
    public bool UseWal { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the cache size in pages.
    /// Default is -1000 (-1 means scaling based on RAM, 1000 is the number of pages).
    /// </summary>
    public int CacheSize { get; set; } = -1000;
    
    /// <summary>
    /// Gets or sets whether to use SQLite connection pooling.
    /// Default is true.
    /// </summary>
    public bool UseConnectionPooling { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// Default is 30 seconds.
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;
}
