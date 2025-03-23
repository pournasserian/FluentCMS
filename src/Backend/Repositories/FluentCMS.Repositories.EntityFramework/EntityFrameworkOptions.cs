using Microsoft.Extensions.Configuration;

namespace FluentCMS.Repositories.EntityFramework;

/// <summary>
/// Configuration options for Entity Framework repositories.
/// </summary>
public class EntityFrameworkOptions
{
    /// <summary>
    /// Gets or sets whether to map camelCase table names to entity names.
    /// By default, table names are PascalCase (same as entity names).
    /// When true, table names will be camelCase.
    /// </summary>
    public bool UseCamelCaseTableNames { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the prefix for table names.
    /// </summary>
    public string? TableNamePrefix { get; set; }
    
    /// <summary>
    /// Gets or sets the suffix for table names.
    /// </summary>
    public string? TableNameSuffix { get; set; }
    
    /// <summary>
    /// Gets or sets the default schema for tables.
    /// </summary>
    public string? DefaultSchema { get; set; }
    
    /// <summary>
    /// Gets or sets whether to automatically run migrations on startup.
    /// </summary>
    public bool AutoMigrateDatabase { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether to use plural table names.
    /// </summary>
    public bool UsePluralTableNames { get; set; } = true;
}
