using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using LiteDB;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.LiteDB;

/// <summary>
/// Repository implementation for LiteDB database provider.
/// </summary>
/// <typeparam name="TEntity">The entity type, which must implement IBaseEntity.</typeparam>
public class LiteDbEntityRepository<TEntity> : IBaseEntityRepository<TEntity> where TEntity : IBaseEntity
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<TEntity> _collection;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the LiteDbEntityRepository class.
    /// </summary>
    /// <param name="options">The LiteDB configuration options.</param>
    public LiteDbEntityRepository(IOptions<LiteDbOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _database = new LiteDatabase(options.Value.ConnectionString);
        _collection = _database.GetCollection<TEntity>(typeof(TEntity).Name);
        
        // Configure collection
        _collection.EnsureIndex(x => x.Id);
    }

    /// <summary>
    /// Creates a new entity in the database.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created entity, or null if creation failed.</returns>
    public async Task<TEntity?> Create(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            
            // Ensure entity has an ID
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            
            // Insert the entity and return it if successful
            return _collection.Insert(entity) != null 
                ? entity 
                : default;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Creates multiple entities in the database.
    /// </summary>
    /// <param name="entities">The entities to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created entities.</returns>
    public async Task<IEnumerable<TEntity>> CreateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities.ToList();
        if (entityList.Count == 0) return [];

        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            
            // Ensure all entities have IDs
            foreach (var entity in entityList)
            {
                if (entity.Id == Guid.Empty)
                {
                    entity.Id = Guid.NewGuid();
                }
            }
            
            // Insert all entities
            _collection.InsertBulk(entityList);
            return entityList;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated entity, or null if the update failed.</returns>
    public async Task<TEntity?> Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (entity.Id == Guid.Empty) throw new ArgumentException("Entity must have a valid ID to be updated.", nameof(entity));
        
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            
            // Update the entity and return it if successful
            return _collection.Update(entity) 
                ? entity 
                : default;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Updates multiple entities in the database.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated entities.</returns>
    public async Task<IEnumerable<TEntity>> UpdateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities.ToList();
        if (entityList.Count == 0) return [];

        // Validate all entities have valid IDs
        if (entityList.Any(e => e.Id == Guid.Empty))
        {
            throw new ArgumentException("All entities must have valid IDs to be updated.");
        }

        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            
            // Update each entity
            var updatedEntities = new List<TEntity>();
            foreach (var entity in entityList)
            {
                if (_collection.Update(entity))
                {
                    updatedEntities.Add(entity);
                }
            }
            
            return updatedEntities;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Deletes an entity from the database by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deleted entity, or null if no entity was found.</returns>
    public async Task<TEntity?> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));
        
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            
            // Get the entity before deleting it
            var entity = _collection.FindById(id);
            if (entity == null) return default;
            
            // Delete the entity and return it if successful
            return _collection.Delete(id) 
                ? entity 
                : default;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Deletes multiple entities from the database by their IDs.
    /// </summary>
    /// <param name="ids">The IDs of the entities to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deleted entities.</returns>
    public async Task<IEnumerable<TEntity>> DeleteMany(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null) throw new ArgumentNullException(nameof(ids));
        
        var idsList = ids.ToList();
        if (!idsList.Any()) return Enumerable.Empty<TEntity>();

        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            
            // Get all entities before deleting them
            var entities = _collection.Find(x => idsList.Contains(x.Id)).ToList();
            if (!entities.Any()) return [];
            
            // Delete each entity
            var deletedEntities = new List<TEntity>();
            foreach (var id in idsList)
            {
                if (_collection.Delete(id))
                {
                    var entity = entities.FirstOrDefault(e => e.Id == id);
                    if (entity != null)
                    {
                        deletedEntities.Add(entity);
                    }
                }
            }
            
            return deletedEntities;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Gets all entities from the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>All entities.</returns>
    public async Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            
            return [.. _collection.FindAll()];
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to get.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity, or null if no entity was found.</returns>
    public async Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));
        
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            
            return _collection.FindById(id);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Gets multiple entities by their IDs.
    /// </summary>
    /// <param name="ids">The IDs of the entities to get.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entities found.</returns>
    public async Task<IEnumerable<TEntity>> GetByIds(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null) throw new ArgumentNullException(nameof(ids));
        
        var idsList = ids.ToList();
        if (idsList.Count == 0) return [];

        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            
            return [.. _collection.Find(x => idsList.Contains(x.Id))];
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    /// <summary>
    /// Finalizes the instance and releases managed resources.
    /// </summary>
    ~LiteDbEntityRepository()
    {
        _database?.Dispose();
        _semaphore?.Dispose();
    }
}
