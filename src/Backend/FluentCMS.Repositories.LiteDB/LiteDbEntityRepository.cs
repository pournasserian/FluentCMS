using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.Core;
using FluentCMS.Repositories.LiteDB.Infrastructure;
using LiteDB;
using System.Linq.Expressions;

namespace FluentCMS.Repositories.LiteDB;

/// <summary>
/// LiteDB implementation of the entity repository.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements IBaseEntity</typeparam>
public class LiteDbEntityRepository<TEntity> : BaseEntityRepositoryBase<TEntity>
    where TEntity : class, IBaseEntity
{
    private readonly LiteDbContext _context;
    private readonly ILiteCollection<TEntity> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteDbEntityRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The LiteDB context.</param>
    public LiteDbEntityRepository(LiteDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _collection = _context.GetCollection<TEntity>();
    }

    #region Basic CRUD Operations

    /// <inheritdoc />
    protected override async Task<TEntity?> CreateInternalAsync(TEntity entity, CancellationToken cancellationToken)
    {
        // LiteDB doesn't support async operations natively, but we use the Task.Run pattern
        // to avoid blocking the calling thread
        return await Task.Run(() =>
        {
            _collection.Insert(entity);
            return entity;
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> CreateManyInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return entitiesList;

            _collection.InsertBulk(entitiesList);
            return entitiesList;
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> UpdateInternalAsync(TEntity entity, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var success = _collection.Update(entity);
            return success ? entity : null;
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> UpdateManyInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return entitiesList;

            // Begin transaction for bulk operations
            _context.BeginTransaction();
            
            try
            {
                foreach (var entity in entitiesList)
                {
                    _collection.Update(entity);
                }
                
                _context.CommitTransaction();
                return entitiesList;
            }
            catch
            {
                _context.RollbackTransaction();
                throw;
            }
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> DeleteInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var entity = _collection.FindById(id);
            if (entity == null)
                return null;

            var success = _collection.Delete(id);
            return success ? entity : null;
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> DeleteManyInternalAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var idsList = ids.ToList();
            if (!idsList.Any())
                return Enumerable.Empty<TEntity>();

            // Get the entities first to return them after deletion
            var entities = _collection.Find(Query.In("_id", idsList.Select(id => new BsonValue(id)).ToArray())).ToList();
            
            if (!entities.Any())
                return Enumerable.Empty<TEntity>();

            // Begin transaction for bulk operations
            _context.BeginTransaction();
            
            try
            {
                foreach (var id in idsList)
                {
                    _collection.Delete(id);
                }
                
                _context.CommitTransaction();
                return entities;
            }
            catch
            {
                _context.RollbackTransaction();
                throw;
            }
        }, cancellationToken);
    }

    #endregion

    #region Query Operations

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllInternalAsync(CancellationToken cancellationToken)
    {
        return await Task.Run(() => _collection.FindAll().ToList(), cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> GetByIdInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _collection.FindById(id), cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetByIdsInternalAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var idsList = ids.ToList();
            if (!idsList.Any())
                return Enumerable.Empty<TEntity>();

            return _collection.Find(Query.In("_id", idsList.Select(id => new BsonValue(id)).ToArray())).ToList();
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> FindInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _collection.Find(predicate).ToList(), cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> FindOneInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _collection.FindOne(predicate), cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<bool> ExistsInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _collection.Exists(predicate), cancellationToken);
    }

    #endregion

    #region Pagination

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedInternalAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var totalCount = _collection.Count();
            var items = _collection.Query()
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
                
            return (items, totalCount);
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedInternalAsync(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var query = _collection.Query().Where(predicate);
            var totalCount = query.Count();
            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
                
            return (items, totalCount);
        }, cancellationToken);
    }

    #endregion

    #region Sorting

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllSortedInternalAsync<TKey>(Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var query = _collection.Query();
            
            if (ascending)
                query = query.OrderBy(keySelector);
            else
                query = query.OrderByDescending(keySelector);
                
            return query.ToList();
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedSortedInternalAsync<TKey>(int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var query = _collection.Query();
            var totalCount = query.Count();
            
            if (ascending)
                query = query.OrderBy(keySelector);
            else
                query = query.OrderByDescending(keySelector);
                
            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
                
            return (items, totalCount);
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> FindSortedInternalAsync<TKey>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var query = _collection.Query().Where(predicate);
            
            if (ascending)
                query = query.OrderBy(keySelector);
            else
                query = query.OrderByDescending(keySelector);
                
            return query.ToList();
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedSortedInternalAsync<TKey>(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var query = _collection.Query().Where(predicate);
            var totalCount = query.Count();
            
            if (ascending)
                query = query.OrderBy(keySelector);
            else
                query = query.OrderByDescending(keySelector);
                
            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
                
            return (items, totalCount);
        }, cancellationToken);
    }

    #endregion

    #region Soft Delete Operations

    /// <inheritdoc />
    protected override async Task<TEntity?> SoftDeleteInternalAsync(Guid id, string? deletedBy, CancellationToken cancellationToken)
    {
        // Check if entity supports soft delete
        if (!typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
            return null;
            
        return await Task.Run(() =>
        {
            var entity = _context.GetCollection<TEntity>(true).FindById(id);
            if (entity == null)
                return null;
                
            // Use runtime type casting to set soft delete properties
            var softDeleteEntity = entity as ISoftDeleteBaseEntity;
            if (softDeleteEntity != null)
            {
                softDeleteEntity.IsDeleted = true;
                softDeleteEntity.DeletedDate = DateTime.UtcNow;
                softDeleteEntity.DeletedBy = deletedBy;
                
                // Update the entity
                var success = _context.GetCollection<TEntity>(true).Update(entity);
                return success ? entity : null;
            }
            
            return null;
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> SoftDeleteManyInternalAsync(IEnumerable<Guid> ids, string? deletedBy, CancellationToken cancellationToken)
    {
        // Check if entity supports soft delete
        if (!typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
            return Enumerable.Empty<TEntity>();
            
        return await Task.Run(() =>
        {
            var idsList = ids.ToList();
            if (!idsList.Any())
                return Enumerable.Empty<TEntity>();
                
            var entities = _context.GetCollection<TEntity>(true)
                .Find(Query.In("_id", idsList.Select(id => new BsonValue(id)).ToArray()))
                .ToList();
                
            if (!entities.Any())
                return Enumerable.Empty<TEntity>();
                
            // Begin transaction for bulk operations
            _context.BeginTransaction();
            
            try
            {
                foreach (var entity in entities)
                {
                    // Use runtime type casting to set soft delete properties
                    var softDeleteEntity = entity as ISoftDeleteBaseEntity;
                    if (softDeleteEntity != null)
                    {
                        softDeleteEntity.IsDeleted = true;
                        softDeleteEntity.DeletedDate = DateTime.UtcNow;
                        softDeleteEntity.DeletedBy = deletedBy;
                        
                        _context.GetCollection<TEntity>(true).Update(entity);
                    }
                }
                
                _context.CommitTransaction();
                return entities;
            }
            catch
            {
                _context.RollbackTransaction();
                throw;
            }
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> RestoreInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        // Check if entity supports soft delete
        if (!typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
            return null;
            
        return await Task.Run(() =>
        {
            var entity = _context.GetCollection<TEntity>(true).FindById(id);
            if (entity == null)
                return null;
                
            // Use runtime type casting to reset soft delete properties
            var softDeleteEntity = entity as ISoftDeleteBaseEntity;
            if (softDeleteEntity != null)
            {
                softDeleteEntity.IsDeleted = false;
                softDeleteEntity.DeletedDate = null;
                softDeleteEntity.DeletedBy = null;
                
                // Update the entity
                var success = _context.GetCollection<TEntity>(true).Update(entity);
                return success ? entity : null;
            }
            
            return null;
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllIncludeDeletedInternalAsync(CancellationToken cancellationToken)
    {
        return await Task.Run(() => _context.GetCollection<TEntity>(true).FindAll().ToList(), cancellationToken);
    }

    #endregion

    #region Aggregation Operations

    /// <inheritdoc />
    protected override async Task<int> CountInternalAsync(CancellationToken cancellationToken)
    {
        return await Task.Run(() => _collection.Count(), cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<int> CountInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _collection.Count(predicate), cancellationToken);
    }

    #endregion

    #region Bulk Update Operations

    /// <inheritdoc />
    protected override async Task<int> UpdateManyWithFieldsInternalAsync(Expression<Func<TEntity, bool>> predicate, Dictionary<string, object> fieldValues, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            if (!fieldValues.Any())
                return 0;
                
            var entities = _collection.Find(predicate).ToList();
            if (!entities.Any())
                return 0;
                
            // Begin transaction for bulk operations
            _context.BeginTransaction();
            
            try
            {
                var updatedCount = 0;
                
                foreach (var entity in entities)
                {
                    var updated = false;
                    
                    // Use reflection to update field values
                    foreach (var kvp in fieldValues)
                    {
                        var property = typeof(TEntity).GetProperty(kvp.Key);
                        if (property != null && property.CanWrite)
                        {
                            property.SetValue(entity, kvp.Value);
                            updated = true;
                        }
                    }
                    
                    if (updated)
                    {
                        _collection.Update(entity);
                        updatedCount++;
                    }
                }
                
                _context.CommitTransaction();
                return updatedCount;
            }
            catch
            {
                _context.RollbackTransaction();
                throw;
            }
        }, cancellationToken);
    }

    #endregion

    #region Projection Support

    /// <inheritdoc />
    protected override async Task<IEnumerable<TResult>> SelectInternalAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _collection.Query().Select(selector).ToList(), cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TResult>> SelectWhereInternalAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken)
    {
        return await Task.Run(() => _collection.Query().Where(predicate).Select(selector).ToList(), cancellationToken);
    }

    #endregion

    #region Async Enumeration

    /// <inheritdoc />
    protected override IAsyncEnumerable<TEntity> GetAllAsStreamInternalAsync(CancellationToken cancellationToken)
    {
        return ConvertToAsyncEnumerable(_collection.FindAll(), cancellationToken);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<TEntity> FindAsStreamInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return ConvertToAsyncEnumerable(_collection.Find(predicate), cancellationToken);
    }

    #endregion

    #region Transaction Support

    /// <inheritdoc />
    protected override async Task<bool> ExecuteInTransactionInternalAsync(Func<IEnhancedBaseEntityRepository<TEntity>, Task<bool>> operation, CancellationToken cancellationToken)
    {
        return await Task.Run(async () =>
        {
            _context.BeginTransaction();
            
            try
            {
                var result = await operation(this);
                
                if (result)
                    _context.CommitTransaction();
                else
                    _context.RollbackTransaction();
                    
                return result;
            }
            catch
            {
                _context.RollbackTransaction();
                throw;
            }
        }, cancellationToken);
    }

    #endregion

    #region Helper Methods

    private async IAsyncEnumerable<T> ConvertToAsyncEnumerable<T>(IEnumerable<T> source, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
            
            // Simulate async behavior to avoid blocking the thread
            await Task.Yield();
        }
    }

    #endregion
}
