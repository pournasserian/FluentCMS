using FluentCMS.Entities;
using FluentCMS.Repositories.EntityFramework;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Repositories.PostgreSQL;

public class PostgreSqlEntityRepository<TEntity> : EntityFrameworkEntityRepository<TEntity> where TEntity : class, IBaseEntity
{
    public PostgreSqlEntityRepository(PostgreSqlDbContext dbContext, ILogger<PostgreSqlEntityRepository<TEntity>> logger) : base(dbContext, logger)
    {
        // PostgreSQL-specific initialization if needed
    }
}
