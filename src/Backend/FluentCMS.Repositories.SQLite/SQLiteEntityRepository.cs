using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.Core;
using FluentCMS.Repositories.SQLite.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FluentCMS.Repositories.SQLite;

/// <summary>
/// SQLite implementation of the entity repository using Entity Framework Core.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements IBaseEntity</typeparam>
public class SQLiteEntityRepository<TEntity> : BaseEntityRepositoryBase<TEntity>
    where TEntity : class, IBaseEntity
{
    private readonly FluentCMSDbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="SQLiteEntityRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public SQLiteEntityRepository(FluentCMSDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.GetDbSet<TEntity>();
    }

    #region Basic CRUD Operations

    /// <inheritdoc />
    protected override async Task<TEntity?> CreateInternalAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> CreateManyInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        var entitiesList = entities.ToList();
        if (!entitiesList.Any())
            return entitiesList;

        await _dbSet.AddRangeAsync(entitiesList, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entitiesList;
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> UpdateInternalAsync(TEntity entity, CancellationToken cancellationToken)
    {
        // Make sure the entity is tracked
        var trackedEntity = await _dbSet.FindAsync(new object[] { entity.Id }, cancellationToken);
        if (trackedEntity == null)
            return null;

        // Update the entity properties
        _dbContext.Entry(trackedEntity).CurrentValues.SetValues(entity);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return trackedEntity;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> UpdateManyInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        var entitiesList = entities.ToList();
        if (!entitiesList.Any())
            return entitiesList;

        var updatedEntities = new List<TEntity>();
        
        foreach (var entity in entitiesList)
        {
            var trackedEntity = await _dbSet.FindAsync(new object[] { entity.Id }, cancellationToken);
            if (trackedEntity != null)
            {
                _dbContext.Entry(trackedEntity).CurrentValues.SetValues(entity);
                updatedEntities.Add(trackedEntity);
            }
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return updatedEntities;
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> DeleteInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity == null)
            return null;

        _dbSet.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> DeleteManyInternalAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idsList = ids.ToList();
        if (!idsList.Any())
            return Enumerable.Empty<TEntity>();

        var entities = await _dbSet.Where(e => idsList.Contains(e.Id)).ToListAsync(cancellationToken);
        if (!entities.Any())
            return Enumerable.Empty<TEntity>();

        _dbSet.RemoveRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entities;
    }

    #endregion

    #region Query Operations

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllInternalAsync(CancellationToken cancellationToken)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> GetByIdInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetByIdsInternalAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idsList = ids.ToList();
        if (!idsList.Any())
            return Enumerable.Empty<TEntity>();

        return await _dbSet.Where(e => idsList.Contains(e.Id)).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> FindInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> FindOneInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<bool> ExistsInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    #endregion

    #region Pagination

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedInternalAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await _dbSet.CountAsync(cancellationToken);
        var items = await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedInternalAsync(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbSet.Where(predicate);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    #endregion

    #region Sorting

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllSortedInternalAsync<TKey>(Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var query = ascending
            ? _dbSet.OrderBy(keySelector)
            : _dbSet.OrderByDescending(keySelector);
            
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedSortedInternalAsync<TKey>(int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var totalCount = await _dbSet.CountAsync(cancellationToken);
        var query = ascending
            ? _dbSet.OrderBy(keySelector)
            : _dbSet.OrderByDescending(keySelector);
            
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> FindSortedInternalAsync<TKey>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var query = ascending
            ? _dbSet.Where(predicate).OrderBy(keySelector)
            : _dbSet.Where(predicate).OrderByDescending(keySelector);
            
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedSortedInternalAsync<TKey>(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var query = _dbSet.Where(predicate);
        var totalCount = await query.CountAsync(cancellationToken);
        var sortedQuery = ascending
            ? query.OrderBy(keySelector)
            : query.OrderByDescending(keySelector);
            
        var items = await sortedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    #endregion

    #region Soft Delete Operations

    /// <inheritdoc />
    protected override async Task<TEntity?> SoftDeleteInternalAsync(Guid id, string? deletedBy, CancellationToken cancellationToken)
    {
        // Check if entity supports soft delete
        if (!typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
            return null;
            
        // Get all entities (including soft-deleted ones)
        var allDbSet = _dbContext.Set<TEntity>().IgnoreQueryFilters();
        
        // Find the entity
        var entity = await allDbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity == null)
            return null;
            
        // Cast to ISoftDeleteBaseEntity and set soft delete properties
        var softDeleteEntity = entity as ISoftDeleteBaseEntity;
        if (softDeleteEntity != null)
        {
            softDeleteEntity.IsDeleted = true;
            softDeleteEntity.DeletedDate = DateTime.UtcNow;
            softDeleteEntity.DeletedBy = deletedBy;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }
        
        return null;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> SoftDeleteManyInternalAsync(IEnumerable<Guid> ids, string? deletedBy, CancellationToken cancellationToken)
    {
        // Check if entity supports soft delete
        if (!typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
            return Enumerable.Empty<TEntity>();
            
        var idsList = ids.ToList();
        if (!idsList.Any())
            return Enumerable.Empty<TEntity>();
            
        // Get all entities (including soft-deleted ones)
        var allDbSet = _dbContext.Set<TEntity>().IgnoreQueryFilters();
        
        // Find the entities
        var entities = await allDbSet.Where(e => idsList.Contains(e.Id)).ToListAsync(cancellationToken);
        if (!entities.Any())
            return Enumerable.Empty<TEntity>();
            
        // Update each entity
        foreach (var entity in entities)
        {
            var softDeleteEntity = entity as ISoftDeleteBaseEntity;
            if (softDeleteEntity != null)
            {
                softDeleteEntity.IsDeleted = true;
                softDeleteEntity.DeletedDate = DateTime.UtcNow;
                softDeleteEntity.DeletedBy = deletedBy;
            }
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entities;
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> RestoreInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        // Check if entity supports soft delete
        if (!typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
            return null;
            
        // Get all entities (including soft-deleted ones)
        var allDbSet = _dbContext.Set<TEntity>().IgnoreQueryFilters();
        
        // Find the entity
        var entity = await allDbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity == null)
            return null;
            
        // Cast to ISoftDeleteBaseEntity and reset soft delete properties
        var softDeleteEntity = entity as ISoftDeleteBaseEntity;
        if (softDeleteEntity != null)
        {
            softDeleteEntity.IsDeleted = false;
            softDeleteEntity.DeletedDate = null;
            softDeleteEntity.DeletedBy = null;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }
        
        return null;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllIncludeDeletedInternalAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Set<TEntity>().IgnoreQueryFilters().ToListAsync(cancellationToken);
    }

    #endregion

    #region Aggregation Operations

    /// <inheritdoc />
    protected override async Task<int> CountInternalAsync(CancellationToken cancellationToken)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<int> CountInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    #endregion

    #region Bulk Update Operations

    /// <inheritdoc />
    protected override async Task<int> UpdateManyWithFieldsInternalAsync(Expression<Func<TEntity, bool>> predicate, Dictionary<string, object> fieldValues, CancellationToken cancellationToken)
    {
        if (!fieldValues.Any())
            return 0;
            
        var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        if (!entities.Any())
            return 0;
            
        var updatedCount = 0;
        
        foreach (var entity in entities)
        {
            var updated = false;
            var entityEntry = _dbContext.Entry(entity);
            
            foreach (var kvp in fieldValues)
            {
                var property = entityEntry.Property(kvp.Key);
                if (property != null)
                {
                    property.CurrentValue = kvp.Value;
                    updated = true;
                }
            }
            
            if (updated)
            {
                updatedCount++;
            }
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return updatedCount;
    }

    #endregion

    #region Projection Support

    /// <inheritdoc />
    protected override async Task<IEnumerable<TResult>> SelectInternalAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken)
    {
        return await _dbSet.Select(selector).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TResult>> SelectWhereInternalAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(predicate).Select(selector).ToListAsync(cancellationToken);
    }

    #endregion

    #region Async Enumeration

    /// <inheritdoc />
    protected override IAsyncEnumerable<TEntity> GetAllAsStreamInternalAsync(CancellationToken cancellationToken)
    {
        return _dbSet.AsAsyncEnumerable();
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<TEntity> FindAsStreamInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return _dbSet.Where(predicate).AsAsyncEnumerable();
    }

    #endregion

    #region Transaction Support

    /// <inheritdoc />
    protected override async Task<bool> ExecuteInTransactionInternalAsync(Func<IEnhancedBaseEntityRepository<TEntity>, Task<bool>> operation, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var result = await operation(this);
            
            if (result)
            {
                await transaction.CommitAsync(cancellationToken);
                return true;
            }
            else
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    #endregion
}
