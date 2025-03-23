using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.EntityFramework;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Repositories.MySQL;

/// <summary>
/// Repository implementation for MySQL database provider.
/// </summary>
/// <typeparam name="TEntity">The entity type, which must implement IBaseEntity.</typeparam>
public class MySqlEntityRepository<TEntity> : EntityFrameworkEntityRepository<TEntity> where TEntity : class, IBaseEntity
{
    /// <summary>
    /// Initializes a new instance of the MySqlEntityRepository class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public MySqlEntityRepository(
        MySqlDbContext dbContext,
        ILogger<MySqlEntityRepository<TEntity>> logger)
        : base(dbContext, logger)
    {
        // MySQL-specific initialization if needed
    }
}
