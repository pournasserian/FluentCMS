using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.EntityFramework;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Repositories.MySQL;

public class MySqlEntityRepository<TEntity> : EntityFrameworkEntityRepository<TEntity> where TEntity : class, IBaseEntity
{
    public MySqlEntityRepository(
        MySqlDbContext dbContext,
        ILogger<MySqlEntityRepository<TEntity>> logger)
        : base(dbContext, logger)
    {
        // MySQL-specific initialization if needed
    }
}
