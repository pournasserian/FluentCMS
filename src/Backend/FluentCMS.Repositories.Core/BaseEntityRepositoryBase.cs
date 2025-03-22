using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using System.Linq.Expressions;

namespace FluentCMS.Repositories.Core;

/// <summary>
/// Base implementation of the IEnhancedBaseEntityRepository that provides
/// common functionality for all repository implementations.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements IBaseEntity</typeparam>
public abstract class BaseEntityRepositoryBase<TEntity> : IEnhancedBaseEntityRepository<TEntity> 
    where TEntity : class, IBaseEntity
{
    #region Abstract Methods (Provider-specific implementations)

    // These methods must be implemented by concrete repository classes

    // Basic CRUD operations
    protected abstract Task<TEntity?> CreateInternalAsync(TEntity entity, CancellationToken cancellationToken);
    protected abstract Task<IEnumerable<TEntity>> CreateManyInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    protected abstract Task<TEntity?> UpdateInternalAsync(TEntity entity, CancellationToken cancellationToken);
    protected abstract Task<IEnumerable<TEntity>> UpdateManyInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    protected abstract Task<TEntity?> DeleteInternalAsync(Guid id, CancellationToken cancellationToken);
    protected abstract Task<IEnumerable<TEntity>> DeleteManyInternalAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    
    // Query operations
    protected abstract Task<IEnumerable<TEntity>> GetAllInternalAsync(CancellationToken cancellationToken);
    protected abstract Task<TEntity?> GetByIdInternalAsync(Guid id, CancellationToken cancellationToken);
    protected abstract Task<IEnumerable<TEntity>> GetByIdsInternalAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    protected abstract Task<IEnumerable<TEntity>> FindInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    protected abstract Task<TEntity?> FindOneInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    protected abstract Task<bool> ExistsInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    
    // Pagination operations
    protected abstract Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedInternalAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    protected abstract Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedInternalAsync(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken);
    
    // Sorting operations
    protected abstract Task<IEnumerable<TEntity>> GetAllSortedInternalAsync<TKey>(Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken);
    protected abstract Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedSortedInternalAsync<TKey>(int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken);
    protected abstract Task<IEnumerable<TEntity>> FindSortedInternalAsync<TKey>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken);
    protected abstract Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedSortedInternalAsync<TKey>(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken);
    
    // Soft delete operations
    protected abstract Task<TEntity?> SoftDeleteInternalAsync(Guid id, string? deletedBy, CancellationToken cancellationToken);
    protected abstract Task<IEnumerable<TEntity>> SoftDeleteManyInternalAsync(IEnumerable<Guid> ids, string? deletedBy, CancellationToken cancellationToken);
    protected abstract Task<TEntity?> RestoreInternalAsync(Guid id, CancellationToken cancellationToken);
    protected abstract Task<IEnumerable<TEntity>> GetAllIncludeDeletedInternalAsync(CancellationToken cancellationToken);
    
    // Aggregation operations
    protected abstract Task<int> CountInternalAsync(CancellationToken cancellationToken);
    protected abstract Task<int> CountInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    
    // Bulk update operations
    protected abstract Task<int> UpdateManyWithFieldsInternalAsync(Expression<Func<TEntity, bool>> predicate, Dictionary<string, object> fieldValues, CancellationToken cancellationToken);
    
    // Projection operations
    protected abstract Task<IEnumerable<TResult>> SelectInternalAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken);
    protected abstract Task<IEnumerable<TResult>> SelectWhereInternalAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken);
    
    // Async enumeration operations
    protected abstract IAsyncEnumerable<TEntity> GetAllAsStreamInternalAsync(CancellationToken cancellationToken);
    protected abstract IAsyncEnumerable<TEntity> FindAsStreamInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    
    // Transaction support
    protected abstract Task<bool> ExecuteInTransactionInternalAsync(Func<IEnhancedBaseEntityRepository<TEntity>, Task<bool>> operation, CancellationToken cancellationToken);

    #endregion

    #region Basic CRUD Operations

    /// <inheritdoc />
    public virtual async Task<TEntity?> Create(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        SetCreationAuditData(entity);
        return await CreateInternalAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> CreateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var entitiesList = entities.ToList();
        
        foreach (var entity in entitiesList)
        {
            SetCreationAuditData(entity);
        }
        
        return await CreateManyInternalAsync(entitiesList, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        SetModificationAuditData(entity);
        return await UpdateInternalAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> UpdateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var entitiesList = entities.ToList();
        
        foreach (var entity in entitiesList)
        {
            SetModificationAuditData(entity);
        }
        
        return await UpdateManyInternalAsync(entitiesList, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<TEntity?> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(id));
            
        return DeleteInternalAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> DeleteMany(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        var idsList = ids.ToList();
        
        if (idsList.Any(id => id == Guid.Empty))
            throw new ArgumentException("Collection contains empty IDs", nameof(ids));
            
        return DeleteManyInternalAsync(idsList, cancellationToken);
    }

    #endregion

    #region Retrieval Operations

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default)
    {
        return GetAllInternalAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(id));
            
        return GetByIdInternalAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> GetByIds(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        var idsList = ids.ToList();
        
        if (idsList.Any(id => id == Guid.Empty))
            throw new ArgumentException("Collection contains empty IDs", nameof(ids));
            
        return GetByIdsInternalAsync(idsList, cancellationToken);
    }

    #endregion

    #region Query Operations

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
            
        return FindInternalAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<TEntity?> FindOne(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
            
        return FindOneInternalAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<bool> Exists(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
            
        return ExistsInternalAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(id));
            
        return await ExistsInternalAsync(e => e.Id == id, cancellationToken);
    }

    #endregion

    #region Pagination

    /// <inheritdoc />
    public virtual Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPaged(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        ValidatePagination(pageNumber, pageSize);
        return GetPagedInternalAsync(pageNumber, pageSize, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPaged(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
            
        ValidatePagination(pageNumber, pageSize);
        return FindPagedInternalAsync(predicate, pageNumber, pageSize, cancellationToken);
    }

    #endregion

    #region Sorting

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> GetAllSorted<TKey>(Expression<Func<TEntity, TKey>> keySelector, bool ascending = true, CancellationToken cancellationToken = default)
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
            
        return GetAllSortedInternalAsync(keySelector, ascending, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedSorted<TKey>(int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending = true, CancellationToken cancellationToken = default)
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
            
        ValidatePagination(pageNumber, pageSize);
        return GetPagedSortedInternalAsync(pageNumber, pageSize, keySelector, ascending, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> FindSorted<TKey>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> keySelector, bool ascending = true, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
            
        return FindSortedInternalAsync(predicate, keySelector, ascending, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedSorted<TKey>(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending = true, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
            
        ValidatePagination(pageNumber, pageSize);
        return FindPagedSortedInternalAsync(predicate, pageNumber, pageSize, keySelector, ascending, cancellationToken);
    }

    #endregion

    #region Soft Delete Operations

    /// <inheritdoc />
    public virtual Task<TEntity?> SoftDelete(Guid id, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(id));
            
        return SoftDeleteInternalAsync(id, deletedBy, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> SoftDeleteMany(IEnumerable<Guid> ids, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        var idsList = ids.ToList();
        
        if (idsList.Any(id => id == Guid.Empty))
            throw new ArgumentException("Collection contains empty IDs", nameof(ids));
            
        return SoftDeleteManyInternalAsync(idsList, deletedBy, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<TEntity?> Restore(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(id));
            
        return RestoreInternalAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> GetAllIncludeDeleted(CancellationToken cancellationToken = default)
    {
        return GetAllIncludeDeletedInternalAsync(cancellationToken);
    }

    #endregion

    #region Aggregation Operations

    /// <inheritdoc />
    public virtual Task<int> Count(CancellationToken cancellationToken = default)
    {
        return CountInternalAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<int> Count(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
            
        return CountInternalAsync(predicate, cancellationToken);
    }

    #endregion

    #region Auditing Extensions

    /// <inheritdoc />
    public virtual async Task<TEntity?> CreateWithAudit(TEntity entity, string createdBy, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
            
        entity.CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        entity.CreatedDate = DateTime.UtcNow;
        
        return await CreateInternalAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> UpdateWithAudit(TEntity entity, string modifiedBy, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
            
        entity.LastModifiedBy = modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy));
        entity.LastModifiedDate = DateTime.UtcNow;
        
        return await UpdateInternalAsync(entity, cancellationToken);
    }

    #endregion

    #region Transaction Support

    /// <inheritdoc />
    public virtual Task<bool> ExecuteInTransaction(Func<IEnhancedBaseEntityRepository<TEntity>, Task<bool>> operation, CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));
            
        return ExecuteInTransactionInternalAsync(operation, cancellationToken);
    }

    #endregion

    #region Bulk Update Operations

    /// <inheritdoc />
    public virtual Task<int> UpdateManyWithFields(Expression<Func<TEntity, bool>> predicate, Dictionary<string, object> fieldValues, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        if (fieldValues == null || !fieldValues.Any())
            throw new ArgumentException("Field values cannot be null or empty", nameof(fieldValues));
            
        // Add audit fields automatically
        if (!fieldValues.ContainsKey(nameof(IBaseEntity.LastModifiedDate)))
            fieldValues.Add(nameof(IBaseEntity.LastModifiedDate), DateTime.UtcNow);
            
        return UpdateManyWithFieldsInternalAsync(predicate, fieldValues, cancellationToken);
    }

    #endregion

    #region Projection Support

    /// <inheritdoc />
    public virtual Task<IEnumerable<TResult>> Select<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        if (selector == null)
            throw new ArgumentNullException(nameof(selector));
            
        return SelectInternalAsync(selector, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<IEnumerable<TResult>> SelectWhere<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        if (selector == null)
            throw new ArgumentNullException(nameof(selector));
            
        return SelectWhereInternalAsync(predicate, selector, cancellationToken);
    }

    #endregion

    #region Async Enumeration

    /// <inheritdoc />
    public virtual IAsyncEnumerable<TEntity> GetAllAsStream(CancellationToken cancellationToken = default)
    {
        return GetAllAsStreamInternalAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual IAsyncEnumerable<TEntity> FindAsStream(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
            
        return FindAsStreamInternalAsync(predicate, cancellationToken);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Sets audit data for entity creation.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    protected virtual void SetCreationAuditData(TEntity entity)
    {
        entity.CreatedDate = DateTime.UtcNow;
        
        // Don't override if already set
        if (string.IsNullOrEmpty(entity.CreatedBy))
            entity.CreatedBy = GetCurrentUserIdentifier();
    }

    /// <summary>
    /// Sets audit data for entity modification.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    protected virtual void SetModificationAuditData(TEntity entity)
    {
        entity.LastModifiedDate = DateTime.UtcNow;
        
        // Don't override if already set
        if (string.IsNullOrEmpty(entity.LastModifiedBy))
            entity.LastModifiedBy = GetCurrentUserIdentifier();
    }

    /// <summary>
    /// Gets the current user identifier for audit purposes.
    /// Override this in derived classes to provide actual user identification.
    /// </summary>
    /// <returns>The current user identifier.</returns>
    protected virtual string? GetCurrentUserIdentifier()
    {
        return "system";
    }

    /// <summary>
    /// Validates pagination parameters.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of entities per page.</param>
    protected virtual void ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than or equal to 1");
            
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 1");
    }

    #endregion
}
