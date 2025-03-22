using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.Core;
using FluentCMS.Repositories.MongoDB.Infrastructure;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace FluentCMS.Repositories.MongoDB;

/// <summary>
/// MongoDB implementation of the entity repository.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements IBaseEntity</typeparam>
public class MongoDbEntityRepository<TEntity> : BaseEntityRepositoryBase<TEntity>
    where TEntity : class, IBaseEntity
{
    private readonly MongoDbContext _context;
    private readonly IMongoCollection<TEntity> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDbEntityRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The MongoDB context.</param>
    public MongoDbEntityRepository(MongoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _collection = _context.GetCollection<TEntity>();
    }

    #region Basic CRUD Operations

    /// <inheritdoc />
    protected override async Task<TEntity?> CreateInternalAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> CreateManyInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        var entitiesList = entities.ToList();
        if (!entitiesList.Any())
            return entitiesList;

        await _collection.InsertManyAsync(entitiesList, cancellationToken: cancellationToken);
        return entitiesList;
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> UpdateInternalAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
        var result = await _collection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = false }, cancellationToken);
        
        return result.ModifiedCount > 0 ? entity : null;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> UpdateManyInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        var entitiesList = entities.ToList();
        if (!entitiesList.Any())
            return entitiesList;

        // Use a session for bulk operations to ensure atomicity
        using var session = await _context.StartSession();
        
        session.StartTransaction();
        
        try
        {
            var bulkOps = new List<WriteModel<TEntity>>();
            
            foreach (var entity in entitiesList)
            {
                var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
                var replaceModel = new ReplaceOneModel<TEntity>(filter, entity);
                bulkOps.Add(replaceModel);
            }
            
            if (bulkOps.Any())
            {
                await _collection.BulkWriteAsync(session, bulkOps, cancellationToken: cancellationToken);
            }
            
            await session.CommitTransactionAsync(cancellationToken);
            return entitiesList;
        }
        catch
        {
            await session.AbortTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> DeleteInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetByIdInternalAsync(id, cancellationToken);
        if (entity == null)
            return null;

        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);
        await _collection.DeleteOneAsync(filter, cancellationToken);
        
        return entity;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> DeleteManyInternalAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idsList = ids.ToList();
        if (!idsList.Any())
            return Enumerable.Empty<TEntity>();

        var entities = await GetByIdsInternalAsync(idsList, cancellationToken);
        var entitiesList = entities.ToList();
        
        if (!entitiesList.Any())
            return Enumerable.Empty<TEntity>();

        var filter = Builders<TEntity>.Filter.In(e => e.Id, idsList);
        await _collection.DeleteManyAsync(filter, cancellationToken);
        
        return entitiesList;
    }

    #endregion

    #region Query Operations

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllInternalAsync(CancellationToken cancellationToken)
    {
        var result = await _collection.Find(Builders<TEntity>.Filter.Empty)
            .ToListAsync(cancellationToken);
            
        return result;
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> GetByIdInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetByIdsInternalAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idsList = ids.ToList();
        if (!idsList.Any())
            return Enumerable.Empty<TEntity>();

        var filter = Builders<TEntity>.Filter.In(e => e.Id, idsList);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> FindInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _collection.Find(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> FindOneInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<bool> ExistsInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _collection.Find(predicate).AnyAsync(cancellationToken);
    }

    #endregion

    #region Pagination

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedInternalAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var filter = Builders<TEntity>.Filter.Empty;
        var totalCount = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        
        var items = await _collection.Find(filter)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, (int)totalCount);
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedInternalAsync(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await _collection.CountDocumentsAsync(predicate, cancellationToken: cancellationToken);
        
        var items = await _collection.Find(predicate)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, (int)totalCount);
    }

    #endregion

    #region Sorting

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllSortedInternalAsync<TKey>(Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var query = _collection.Find(Builders<TEntity>.Filter.Empty);
        
        if (ascending)
            query = query.SortBy(keySelector);
        else
            query = query.SortByDescending(keySelector);
            
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedSortedInternalAsync<TKey>(int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var filter = Builders<TEntity>.Filter.Empty;
        var totalCount = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        
        var query = _collection.Find(filter);
        
        if (ascending)
            query = query.SortBy(keySelector);
        else
            query = query.SortByDescending(keySelector);
            
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, (int)totalCount);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> FindSortedInternalAsync<TKey>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var query = _collection.Find(predicate);
        
        if (ascending)
            query = query.SortBy(keySelector);
        else
            query = query.SortByDescending(keySelector);
            
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedSortedInternalAsync<TKey>(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var totalCount = await _collection.CountDocumentsAsync(predicate, cancellationToken: cancellationToken);
        
        var query = _collection.Find(predicate);
        
        if (ascending)
            query = query.SortBy(keySelector);
        else
            query = query.SortByDescending(keySelector);
            
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, (int)totalCount);
    }

    #endregion

    #region Soft Delete Operations

    /// <inheritdoc />
    protected override async Task<TEntity?> SoftDeleteInternalAsync(Guid id, string? deletedBy, CancellationToken cancellationToken)
    {
        // Check if entity supports soft delete
        if (!typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
            return null;
            
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);
        var entity = await _context.GetCollection<TEntity>(true).Find(filter).FirstOrDefaultAsync(cancellationToken);
        
        if (entity == null)
            return null;
            
        // Use reflection to set soft delete properties
        var softDeleteEntity = entity as ISoftDeleteBaseEntity;
        if (softDeleteEntity != null)
        {
            softDeleteEntity.IsDeleted = true;
            softDeleteEntity.DeletedDate = DateTime.UtcNow;
            softDeleteEntity.DeletedBy = deletedBy;
            
            // Update the entity
            await _context.GetCollection<TEntity>(true).ReplaceOneAsync(
                filter,
                entity,
                new ReplaceOptions { IsUpsert = false },
                cancellationToken);
                
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
            
        var filter = Builders<TEntity>.Filter.In(e => e.Id, idsList);
        var entities = await _context.GetCollection<TEntity>(true).Find(filter).ToListAsync(cancellationToken);
        
        if (!entities.Any())
            return Enumerable.Empty<TEntity>();
            
        // Use a session for bulk operations to ensure atomicity
        using var session = await _context.StartSession();
        
        session.StartTransaction();
        
        try
        {
            var bulkOps = new List<WriteModel<TEntity>>();
            
            foreach (var entity in entities)
            {
                // Use reflection to set soft delete properties
                var softDeleteEntity = entity as ISoftDeleteBaseEntity;
                if (softDeleteEntity != null)
                {
                    softDeleteEntity.IsDeleted = true;
                    softDeleteEntity.DeletedDate = DateTime.UtcNow;
                    softDeleteEntity.DeletedBy = deletedBy;
                    
                    var entityFilter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
                    var replaceModel = new ReplaceOneModel<TEntity>(entityFilter, entity);
                    bulkOps.Add(replaceModel);
                }
            }
            
            if (bulkOps.Any())
            {
                await _context.GetCollection<TEntity>(true).BulkWriteAsync(session, bulkOps, cancellationToken: cancellationToken);
            }
            
            await session.CommitTransactionAsync(cancellationToken);
            return entities;
        }
        catch
        {
            await session.AbortTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> RestoreInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        // Check if entity supports soft delete
        if (!typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
            return null;
            
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);
        var entity = await _context.GetCollection<TEntity>(true).Find(filter).FirstOrDefaultAsync(cancellationToken);
        
        if (entity == null)
            return null;
            
        // Use reflection to reset soft delete properties
        var softDeleteEntity = entity as ISoftDeleteBaseEntity;
        if (softDeleteEntity != null)
        {
            softDeleteEntity.IsDeleted = false;
            softDeleteEntity.DeletedDate = null;
            softDeleteEntity.DeletedBy = null;
            
            // Update the entity
            await _context.GetCollection<TEntity>(true).ReplaceOneAsync(
                filter,
                entity,
                new ReplaceOptions { IsUpsert = false },
                cancellationToken);
                
            return entity;
        }
        
        return null;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllIncludeDeletedInternalAsync(CancellationToken cancellationToken)
    {
        // Use the unfiltered collection to include soft-deleted entities
        return await _context.GetCollection<TEntity>(true)
            .Find(Builders<TEntity>.Filter.Empty)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Aggregation Operations

    /// <inheritdoc />
    protected override async Task<int> CountInternalAsync(CancellationToken cancellationToken)
    {
        var count = await _collection.CountDocumentsAsync(Builders<TEntity>.Filter.Empty, cancellationToken: cancellationToken);
        return (int)count;
    }

    /// <inheritdoc />
    protected override async Task<int> CountInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        var count = await _collection.CountDocumentsAsync(predicate, cancellationToken: cancellationToken);
        return (int)count;
    }

    #endregion

    #region Bulk Update Operations

    /// <inheritdoc />
    protected override async Task<int> UpdateManyWithFieldsInternalAsync(Expression<Func<TEntity, bool>> predicate, Dictionary<string, object> fieldValues, CancellationToken cancellationToken)
    {
        var updateDefinitions = new List<UpdateDefinition<TEntity>>();
        
        foreach (var kvp in fieldValues)
        {
            updateDefinitions.Add(Builders<TEntity>.Update.Set(kvp.Key, kvp.Value));
        }
        
        if (!updateDefinitions.Any())
            return 0;
            
        var update = Builders<TEntity>.Update.Combine(updateDefinitions);
        var result = await _collection.UpdateManyAsync(predicate, update, cancellationToken: cancellationToken);
        
        return (int)result.ModifiedCount;
    }

    #endregion

    #region Projection Support

    /// <inheritdoc />
    protected override async Task<IEnumerable<TResult>> SelectInternalAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken)
    {
        return await _collection.Find(Builders<TEntity>.Filter.Empty)
            .Project(selector)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TResult>> SelectWhereInternalAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken)
    {
        return await _collection.Find(predicate)
            .Project(selector)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Async Enumeration

    /// <inheritdoc />
    protected override IAsyncEnumerable<TEntity> GetAllAsStreamInternalAsync(CancellationToken cancellationToken)
    {
        return _collection.Find(Builders<TEntity>.Filter.Empty)
            .ToAsyncEnumerable(cancellationToken);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<TEntity> FindAsStreamInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return _collection.Find(predicate)
            .ToAsyncEnumerable(cancellationToken);
    }

    #endregion

    #region Transaction Support

    /// <inheritdoc />
    protected override async Task<bool> ExecuteInTransactionInternalAsync(Func<IEnhancedBaseEntityRepository<TEntity>, Task<bool>> operation, CancellationToken cancellationToken)
    {
        using var session = await _context.StartSession();
        
        session.StartTransaction();
        
        try
        {
            var result = await operation(this);
            
            if (result)
                await session.CommitTransactionAsync(cancellationToken);
            else
                await session.AbortTransactionAsync(cancellationToken);
                
            return result;
        }
        catch
        {
            await session.AbortTransactionAsync(cancellationToken);
            throw;
        }
    }

    #endregion
}
