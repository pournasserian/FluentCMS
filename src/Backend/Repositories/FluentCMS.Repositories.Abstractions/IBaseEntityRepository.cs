using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions.Querying;

namespace FluentCMS.Repositories.Abstractions;

public interface IBaseEntityRepository<TEntity> where TEntity : IBaseEntity
{
    Task<TEntity?> Create(TEntity entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> CreateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<TEntity?> Update(TEntity entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> UpdateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<TEntity?> Delete(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> DeleteMany(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default);
    Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetByIds(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves a paged list of entities based on the provided query parameters.
    /// </summary>
    /// <param name="queryParameters">The query parameters including filtering, sorting, and pagination options.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paged result containing the requested entities and pagination metadata.</returns>
    Task<PagedResult<TEntity>> QueryAsync(
        QueryParameters<TEntity>? queryParameters = null,
        CancellationToken cancellationToken = default);
}
