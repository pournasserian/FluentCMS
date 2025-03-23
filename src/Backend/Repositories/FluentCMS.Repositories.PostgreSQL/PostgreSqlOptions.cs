using FluentCMS.Repositories.EntityFramework;

namespace FluentCMS.Repositories.PostgreSQL;

/// <summary>
/// Configuration options for PostgreSQL database provider.
/// </summary>
public class PostgreSqlOptions : EntityFrameworkOptions
{
    /// <summary>
    /// Gets or sets the connection string for the PostgreSQL database.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the server version for PostgreSQL.
    /// If not specified, it will be auto-detected from the connection string.
    /// Format: Major.Minor[.Build] (e.g., "15.3" or "14.8.0").
    /// </summary>
    public string? ServerVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use connection pooling.
    /// Default is true.
    /// </summary>
    public bool UseConnectionPooling { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of connections allowed in the connection pool.
    /// Default is 100.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the minimum number of connections to maintain in the connection pool.
    /// Default is 0.
    /// </summary>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// Default is 30 seconds.
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL for database connections.
    /// Default is false.
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Gets or sets the SSL mode for PostgreSQL connection.
    /// Options: Disable, Allow, Prefer, Require, VerifyCA, VerifyFull
    /// Default is "Prefer".
    /// </summary>
    public string SslMode { get; set; } = "Prefer";

    /// <summary>
    /// Gets or sets the database schema to use.
    /// If not specified, the default schema will be used.
    /// </summary>
    public string? Schema { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable retrying on failure.
    /// Default is true.
    /// </summary>
    public bool EnableRetryOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// Default is 5.
    /// </summary>
    public int MaxRetryCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether to enable batched commands.
    /// Default is true.
    /// </summary>
    public bool EnableBatchCommands { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use the PostgreSQL-specific
    /// JSON capabilities for storing and querying complex properties.
    /// Default is false.
    /// </summary>
    public bool UseJsonForComplexTypes { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to use case-insensitive collation
    /// for text columns. This affects sorting and comparison operations.
    /// Default is true.
    /// </summary>
    public bool UseCaseInsensitiveCollation { get; set; } = true;
}
