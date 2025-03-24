namespace FluentCMS.Repositories.MySQL;

using FluentCMS.Repositories.EntityFramework;

public class MySqlOptions : EntityFrameworkOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string? ServerVersion { get; set; }

    public bool UseConnectionPooling { get; set; } = true;

    public string CharacterSet { get; set; } = "utf8mb4";

    public int ConnectionTimeout { get; set; } = 30;

    public int MaxPoolSize { get; set; } = 100;

    public int MinPoolSize { get; set; } = 0;

    public bool UseSsl { get; set; } = false;
}
