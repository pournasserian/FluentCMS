using FluentCMS.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.SQLite;

public class SqliteDbContext : FluentCmsDbContext
{
    private readonly SqliteOptions _sqliteOptions;

    public SqliteDbContext(DbContextOptions<SqliteDbContext> options, IOptions<SqliteOptions> sqliteOptions) : base(options, sqliteOptions)
    {
        _sqliteOptions = sqliteOptions?.Value ?? throw new ArgumentNullException(nameof(sqliteOptions));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Only configure if not already configured
            ConfigureSqlite(optionsBuilder);
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply SQLite-specific configurations
        // SQLite doesn't support certain SQL Server features, so we need to configure workarounds

        // For example, SQLite doesn't support decimal precision, so we need to configure it as text
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("TEXT");
        }

        base.OnModelCreating(modelBuilder);
    }

    private void ConfigureSqlite(DbContextOptionsBuilder optionsBuilder)
    {
        // Build connection string
        var connectionString = BuildConnectionString();

        // Configure SQLite
        optionsBuilder.UseSqlite(connectionString, sqliteOptions =>
        {
            // Configure migrations assembly if needed
            // sqliteOptions.MigrationsAssembly("FluentCMS.Repositories.SQLite");

            // Configure connection timeout
            sqliteOptions.CommandTimeout(_sqliteOptions.ConnectionTimeout);
        });
    }

    private string BuildConnectionString()
    {
        // Start with the data source
        var connectionStringBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = _sqliteOptions.DatabasePath,
            Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate,
            Cache = _sqliteOptions.UseConnectionPooling ? Microsoft.Data.Sqlite.SqliteCacheMode.Shared : Microsoft.Data.Sqlite.SqliteCacheMode.Private,
            ForeignKeys = _sqliteOptions.EnableForeignKeys
        };

        // Add additional pragmas as query string parameters
        var connectionString = connectionStringBuilder.ToString();

        // Add WAL mode if enabled
        if (_sqliteOptions.UseWal)
        {
            connectionString += ";Journal Mode=WAL";
        }

        // Add cache size
        connectionString += $";Cache Size={_sqliteOptions.CacheSize}";

        return connectionString;
    }
}
