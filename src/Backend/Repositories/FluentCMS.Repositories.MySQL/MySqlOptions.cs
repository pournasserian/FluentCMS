namespace FluentCMS.Repositories.MySQL;

using FluentCMS.Repositories.EntityFramework;

/// <summary>
/// Configuration options for MySQL repositories.
/// </summary>
public class MySqlOptions : EntityFrameworkOptions
{
    /// <summary>
    /// Gets or sets the MySQL connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the MySQL server version.
    /// This is important for Pomelo.EntityFrameworkCore.MySql to generate compatible SQL.
    /// Example format: "8.0.28" or "5.7.38"
    /// </summary>
    public string? ServerVersion { get; set; }
    
    /// <summary>
    /// Gets or sets whether to use MySQL connection pooling.
    /// Default is true.
    /// </summary>
    public bool UseConnectionPooling { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the default character set.
    /// Default is "utf8mb4" for full Unicode support.
    /// </summary>
    public string CharacterSet { get; set; } = "utf8mb4";
    
    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// Default is 30 seconds.
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;
    
    /// <summary>
    /// Gets or sets the maximum pool size for connections.
    /// Default is 100.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;
    
    /// <summary>
    /// Gets or sets the minimum pool size for connections.
    /// Default is 0.
    /// </summary>
    public int MinPoolSize { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets whether to use SSL/TLS for database connections.
    /// Default is false.
    /// </summary>
    public bool UseSsl { get; set; } = false;
}
