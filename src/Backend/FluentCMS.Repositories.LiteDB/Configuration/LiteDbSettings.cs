using System;

namespace FluentCMS.Repositories.LiteDB.Configuration;

/// <summary>
/// Settings for LiteDB connection and configuration.
/// </summary>
public class LiteDbSettings
{
    /// <summary>
    /// Gets or sets the connection string for LiteDB.
    /// This is typically the path to the database file.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether to apply a global soft-delete filter.
    /// </summary>
    public bool ApplySoftDeleteFilter { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether to pluralize collection names.
    /// </summary>
    public bool PluralizeCollectionNames { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the prefix for collection names.
    /// </summary>
    public string? CollectionNamePrefix { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to auto-generate indexes for Id properties.
    /// </summary>
    public bool AutoGenerateIndexes { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether the database should be upgraded automatically.
    /// </summary>
    public bool UpgradeDatabase { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether the database has a shared connection or each repository has its own connection.
    /// Shared connection is better for performance but requires more careful handling in multi-threaded scenarios.
    /// </summary>
    public bool UseSharedConnection { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the initial database size in MB. Default is 0, which means no specific initial size.
    /// </summary>
    public int InitialSize { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets a value indicating whether the database file should be created if it doesn't exist.
    /// </summary>
    public bool CreateIfNotExists { get; set; } = true;
    
    /// <summary>
    /// Validates the settings.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when required settings are not provided.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new ArgumentException("LiteDB connection string is required.", nameof(ConnectionString));
    }
}
