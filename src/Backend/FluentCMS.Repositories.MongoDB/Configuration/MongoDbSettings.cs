using System;

namespace FluentCMS.Repositories.MongoDB.Configuration;

/// <summary>
/// Settings for MongoDB connection and configuration.
/// </summary>
public class MongoDbSettings
{
    /// <summary>
    /// Gets or sets the connection string for MongoDB.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;
    
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
    /// Validates the settings.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when required settings are not provided.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new ArgumentException("MongoDB connection string is required.", nameof(ConnectionString));
            
        if (string.IsNullOrWhiteSpace(DatabaseName))
            throw new ArgumentException("MongoDB database name is required.", nameof(DatabaseName));
    }
}
