namespace FluentCMS.Repositories.SqlServer;

using FluentCMS.Repositories.EntityFramework;

/// <summary>
/// Configuration options for SQL Server repositories.
/// </summary>
public class SqlServerOptions : EntityFrameworkOptions
{
    /// <summary>
    /// Gets or sets the SQL Server connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether to enable retry on failure.
    /// Default is true.
    /// </summary>
    public bool EnableRetryOnFailure { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// Default is 5.
    /// </summary>
    public int MaxRetryCount { get; set; } = 5;
    
    /// <summary>
    /// Gets or sets the maximum time to retry, in seconds.
    /// Default is 30 seconds.
    /// </summary>
    public int MaxRetryDelay { get; set; } = 30;
    
    /// <summary>
    /// Gets or sets the command timeout, in seconds.
    /// Default is 30 seconds.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
    
    /// <summary>
    /// Gets or sets whether to enable sensitive data logging.
    /// Default is false for security.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether to enable detailed errors.
    /// Default is false in production.
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether to use MARS (Multiple Active Result Sets).
    /// Default is false.
    /// </summary>
    public bool EnableMultipleActiveResultSets { get; set; } = false;
}
