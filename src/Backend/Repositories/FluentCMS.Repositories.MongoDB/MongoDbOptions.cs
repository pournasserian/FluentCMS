using Microsoft.Extensions.Configuration;

namespace FluentCMS.Repositories.MongoDB;

public class MongoDbOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    
    public string DatabaseName { get; set; } = string.Empty;
    
    public bool UseCamelCaseCollectionNames { get; set; } = false;
    
    public string? CollectionNamePrefix { get; set; }
    
    public string? CollectionNameSuffix { get; set; }
}
