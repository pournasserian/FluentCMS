namespace FluentCMS.Repositories.SqlServer;

using FluentCMS.Repositories.EntityFramework;

public class SqlServerOptions : EntityFrameworkOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    
    public bool EnableRetryOnFailure { get; set; } = true;
    
    public int MaxRetryCount { get; set; } = 5;
    
    public int MaxRetryDelay { get; set; } = 30;
    
    public int CommandTimeout { get; set; } = 30;
    
    public bool EnableSensitiveDataLogging { get; set; } = false;
    
    public bool EnableDetailedErrors { get; set; } = false;
    
    public bool EnableMultipleActiveResultSets { get; set; } = false;
}
