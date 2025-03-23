using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.EntityFramework;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Repositories.PostgreSQL;

/// <summary>
/// Repository implementation for PostgreSQL database provider.
/// </summary>
/// <typeparam name="TEntity">The entity type, which must implement IBaseEntity.</typeparam>
public class PostgreSqlEntityRepository<TEntity> : EntityFrameworkEntityRepository<TEntity> where TEntity : class, IBaseEntity
{
    /// <summary>
    /// Initializes a new instance of the PostgreSqlEntityRepository class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public PostgreSqlEntityRepository(
        PostgreSqlDbContext dbContext,
        ILogger<PostgreSqlEntityRepository<TEntity>> logger)
        : base(dbContext, logger)
    {
        // PostgreSQL-specific initialization if needed
    }
}
