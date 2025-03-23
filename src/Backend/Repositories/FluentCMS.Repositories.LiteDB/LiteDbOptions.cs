namespace FluentCMS.Repositories.LiteDB;

/// <summary>
/// Provides configuration options for LiteDB repository.
/// </summary>
public class LiteDbOptions
{
    /// <summary>
    /// Gets or sets the connection string for the LiteDB database.
    /// Default is "Filename=fluentcms.db;Mode=Shared".
    /// </summary>
    public string ConnectionString { get; set; } = "Filename=fluentcms.db;Mode=Shared";
}
