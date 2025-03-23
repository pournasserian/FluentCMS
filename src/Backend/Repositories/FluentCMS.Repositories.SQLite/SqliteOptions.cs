namespace FluentCMS.Repositories.SQLite;

using FluentCMS.Repositories.EntityFramework;

public class SqliteOptions : EntityFrameworkOptions
{
    public string DatabasePath { get; set; } = "fluentcms.db";
    
    public bool EnableForeignKeys { get; set; } = true;
    
    public bool UseWal { get; set; } = true;
    
    public int CacheSize { get; set; } = -1000;
    
    public bool UseConnectionPooling { get; set; } = true;
    
    public int ConnectionTimeout { get; set; } = 30;
}
