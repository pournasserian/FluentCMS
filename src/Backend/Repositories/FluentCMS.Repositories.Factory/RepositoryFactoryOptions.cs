using FluentCMS.Repositories.EntityFramework;
using FluentCMS.Repositories.LiteDB;
using FluentCMS.Repositories.MongoDB;
using FluentCMS.Repositories.SQLite;
using FluentCMS.Repositories.SqlServer;

namespace FluentCMS.Repositories.Factory;

/// <summary>
/// Options for configuring the Repository Factory.
/// </summary>
public class RepositoryFactoryOptions
{
    /// <summary>
    /// Gets or sets the provider type to use. 
    /// Valid values: "MongoDB", "LiteDB", "EntityFramework", "SQLite", "SqlServer"
    /// </summary>
    public string Provider { get; set; } = "MongoDB";
    
    /// <summary>
    /// Gets or sets the MongoDB provider options.
    /// Used when Provider is set to "MongoDB".
    /// </summary>
    public MongoDbOptions MongoDB { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the LiteDB provider options.
    /// Used when Provider is set to "LiteDB".
    /// </summary>
    public LiteDbOptions LiteDB { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the Entity Framework provider options.
    /// Used when Provider is set to "EntityFramework".
    /// </summary>
    public EntityFrameworkOptions EntityFramework { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the SQLite provider options.
    /// Used when Provider is set to "SQLite".
    /// </summary>
    public SqliteOptions SQLite { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the SQL Server provider options.
    /// Used when Provider is set to "SqlServer".
    /// </summary>
    public SqlServerOptions SqlServer { get; set; } = new();
}
