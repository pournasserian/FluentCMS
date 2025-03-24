using FluentCMS.Repositories.EntityFramework;
using FluentCMS.Repositories.LiteDB;
using FluentCMS.Repositories.MongoDB;
using FluentCMS.Repositories.MySQL;
using FluentCMS.Repositories.PostgreSQL;
using FluentCMS.Repositories.SQLite;
using FluentCMS.Repositories.SqlServer;

namespace FluentCMS.Repositories.Factory;

public class RepositoryFactoryOptions
{
    public string Provider { get; set; } = "MongoDB";

    public MongoDbOptions MongoDB { get; set; } = new();

    public LiteDbOptions LiteDB { get; set; } = new();

    public EntityFrameworkOptions EntityFramework { get; set; } = new();

    public SqliteOptions SQLite { get; set; } = new();

    public SqlServerOptions SqlServer { get; set; } = new();

    public MySqlOptions MySQL { get; set; } = new();

    public PostgreSqlOptions PostgreSQL { get; set; } = new();
}
