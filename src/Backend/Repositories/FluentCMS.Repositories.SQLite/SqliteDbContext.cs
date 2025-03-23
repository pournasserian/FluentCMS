using FluentCMS.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.SQLite;

/// <summary>
/// SQLite implementation of the FluentCMS DbContext.
/// </summary>
public class SqliteDbContext : FluentCmsDbContext
{
    private readonly SqliteOptions _sqliteOptions;

    /// <summary>
    /// Initializes a new instance of the SqliteDbContext class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    /// <param name="sqliteOptions">The SQLite configuration options.</param>
    public SqliteDbContext(
        DbContextOptions<SqliteDbContext> options,
        IOptions<SqliteOptions> sqliteOptions)
        : base(options, sqliteOptions)
    {
        _sqliteOptions = sqliteOptions?.Value ?? throw new ArgumentNullException(nameof(sqliteOptions));
    }

    /// <summary>
    /// Configures SQLite-specific options when the context is being configured.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Only configure if not already configured
            ConfigureSqlite(optionsBuilder);
        }

        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    /// Configures SQLite-specific model creation options.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
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

    /// <summary>
    /// Applies SQLite-specific configuration to the DbContext options.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
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

    /// <summary>
    /// Builds the SQLite connection string based on the options.
    /// </summary>
    /// <returns>The connection string.</returns>
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
