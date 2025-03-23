using Microsoft.Extensions.Configuration;

namespace FluentCMS.Repositories.EntityFramework;

public class EntityFrameworkOptions
{
    public bool UseCamelCaseTableNames { get; set; } = false;
    
    public string? TableNamePrefix { get; set; }
    
    public string? TableNameSuffix { get; set; }
    
    public string? DefaultSchema { get; set; }
    
    public bool AutoMigrateDatabase { get; set; } = false;
    
    public bool UsePluralTableNames { get; set; } = true;
}
