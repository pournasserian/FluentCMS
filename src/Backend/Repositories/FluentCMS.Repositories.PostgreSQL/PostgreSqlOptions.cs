using FluentCMS.Repositories.EntityFramework;

namespace FluentCMS.Repositories.PostgreSQL;

public class PostgreSqlOptions : EntityFrameworkOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string? ServerVersion { get; set; }

    public bool UseConnectionPooling { get; set; } = true;

    public int MaxPoolSize { get; set; } = 100;

    public int MinPoolSize { get; set; } = 0;

    public int ConnectionTimeout { get; set; } = 30;

    public bool UseSsl { get; set; } = false;

    public string SslMode { get; set; } = "Prefer";

    public string? Schema { get; set; }

    public bool EnableRetryOnFailure { get; set; } = true;

    public int MaxRetryCount { get; set; } = 5;

    public bool EnableBatchCommands { get; set; } = true;

    public bool UseJsonForComplexTypes { get; set; } = false;

    public bool UseCaseInsensitiveCollation { get; set; } = true;
}
