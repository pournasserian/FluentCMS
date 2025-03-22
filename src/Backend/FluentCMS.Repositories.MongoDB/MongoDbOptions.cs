using Microsoft.Extensions.Configuration;

namespace FluentCMS.Repositories.MongoDB;

/// <summary>
/// Configuration options for MongoDB repositories.
/// </summary>
public class MongoDbOptions
{
    /// <summary>
    /// Gets or sets the MongoDB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the MongoDB database name.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether to map camelCase collection names to entity names.
    /// By default, collection names are PascalCase (same as entity names).
    /// When true, collection names will be camelCase.
    /// </summary>
    public bool UseCamelCaseCollectionNames { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the prefix for collection names.
    /// </summary>
    public string? CollectionNamePrefix { get; set; }
    
    /// <summary>
    /// Gets or sets the suffix for collection names.
    /// </summary>
    public string? CollectionNameSuffix { get; set; }
}
