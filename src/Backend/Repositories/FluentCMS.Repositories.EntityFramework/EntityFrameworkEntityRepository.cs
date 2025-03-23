using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Repositories.EntityFramework;

/// <summary>
/// Repository implementation for Entity Framework database providers.
/// </summary>
/// <typeparam name="TEntity">The entity type, which must implement IBaseEntity.</typeparam>
public class EntityFrameworkEntityRepository<TEntity> : IBaseEntityRepository<TEntity> where TEntity : class, IBaseEntity
{
    protected readonly FluentCmsDbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger<EntityFrameworkEntityRepository<TEntity>> _logger;

    /// <summary>
    /// Initializes a new instance of the EntityFrameworkEntityRepository class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public EntityFrameworkEntityRepository(
        FluentCmsDbContext dbContext,
        ILogger<EntityFrameworkEntityRepository<TEntity>> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = dbContext.Set<TEntity>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new entity in the database.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created entity, or null if creation failed.</returns>
    public async Task<TEntity?> Create(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            // Ensure entity has an ID
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            // Set audit fields if entity is IAuditableEntity
            if (entity is IAuditableEntity auditableEntity)
            {
                auditableEntity.CreatedDate = DateTime.UtcNow;
            }

            // Add the entity to the context
            await _dbSet.AddAsync(entity, cancellationToken);
            
            // Save changes to the database
            await _dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity of type {EntityType}", typeof(TEntity).Name);
            return default;
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
            // Ensure all entities have IDs and set audit fields if applicable
            foreach (var entity in entityList)
            {
                if (entity.Id == Guid.Empty)
                {
                    entity.Id = Guid.NewGuid();
                }

                // Set audit fields if entity is IAuditableEntity
                if (entity is IAuditableEntity auditableEntity)
                {
                    auditableEntity.CreatedDate = DateTime.UtcNow;
                }
            }

            // Add all entities to the context
            await _dbSet.AddRangeAsync(entityList, cancellationToken);
            
            // Save changes to the database
            await _dbContext.SaveChangesAsync(cancellationToken);

            return entityList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating multiple entities of type {EntityType}", typeof(TEntity).Name);
            return [];
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
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.Id == Guid.Empty) throw new ArgumentException("Entity must have a valid ID to be updated.", nameof(entity));

        try
        {
            // Check if entity exists
            var existingEntity = await _dbSet.FindAsync([entity.Id], cancellationToken);
            if (existingEntity == null) return default;

            // Set update audit fields if entity is IAuditableEntity
            if (entity is IAuditableEntity auditableEntity)
            {
                auditableEntity.LastModifiedDate = DateTime.UtcNow;
                
                // Preserve creation audit data if the existing entity has it
                if (existingEntity is IAuditableEntity existingAuditableEntity)
                {
                    auditableEntity.CreatedDate = existingAuditableEntity.CreatedDate;
                    auditableEntity.CreatedBy = existingAuditableEntity.CreatedBy;
                }
            }

            // Detach the existing entity from the context
            _dbContext.Entry(existingEntity).State = EntityState.Detached;
            
            // Update the entity
            _dbContext.Entry(entity).State = EntityState.Modified;
            
            // Save changes to the database
            await _dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity of type {EntityType} with ID {EntityId}",
                typeof(TEntity).Name, entity.Id);
            return default;
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
            // Get IDs of all entities to update
            var ids = entityList.Select(e => e.Id).ToList();
            
            // Get existing entities in a single query for efficiency
            var existingEntities = await _dbSet
                .Where(e => ids.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, cancellationToken);

            var updatedEntities = new List<TEntity>();
            foreach (var entity in entityList)
            {
                // Check if entity exists
                if (!existingEntities.TryGetValue(entity.Id, out var existingEntity))
                {
                    continue;
                }

                // Set update audit fields if entity is IAuditableEntity
                if (entity is IAuditableEntity auditableEntity)
                {
                    auditableEntity.LastModifiedDate = DateTime.UtcNow;
                    
                    // Preserve creation audit data if the existing entity has it
                    if (existingEntity is IAuditableEntity existingAuditableEntity)
                    {
                        auditableEntity.CreatedDate = existingAuditableEntity.CreatedDate;
                        auditableEntity.CreatedBy = existingAuditableEntity.CreatedBy;
                    }
                }

                // Detach the existing entity from the context
                _dbContext.Entry(existingEntity).State = EntityState.Detached;
                
                // Update the entity
                _dbContext.Entry(entity).State = EntityState.Modified;
                
                updatedEntities.Add(entity);
            }

            if (updatedEntities.Count > 0)
            {
                // Save changes to the database
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return updatedEntities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating multiple entities of type {EntityType}",
                typeof(TEntity).Name);
            return [];
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
            // Find the entity before deletion
            var entity = await _dbSet.FindAsync([id], cancellationToken);
            if (entity == null) return default;

            // Remove the entity
            _dbSet.Remove(entity);
            
            // Save changes to the database
            await _dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity of type {EntityType} with ID {EntityId}",
                typeof(TEntity).Name, id);
            return default;
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
        ArgumentNullException.ThrowIfNull(ids);

        var idsList = ids.ToList();
        if (idsList.Count == 0) return [];

        try
        {
            // Find entities to delete
            var entities = await _dbSet
                .Where(e => idsList.Contains(e.Id))
                .ToListAsync(cancellationToken);
                
            if (entities.Count == 0) return [];

            // Remove the entities
            _dbSet.RemoveRange(entities);
            
            // Save changes to the database
            await _dbContext.SaveChangesAsync(cancellationToken);

            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple entities of type {EntityType}",
                typeof(TEntity).Name);
            return [];
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
            return await _dbSet.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entities of type {EntityType}",
                typeof(TEntity).Name);
            return [];
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
            return await _dbSet.FindAsync([id], cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity of type {EntityType} with ID {EntityId}",
                typeof(TEntity).Name, id);
            return default;
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
        ArgumentNullException.ThrowIfNull(ids);

        var idsList = ids.ToList();
        if (idsList.Count == 0) return [];

        try
        {
            return await _dbSet
                .Where(e => idsList.Contains(e.Id))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple entities of type {EntityType}",
                typeof(TEntity).Name);
            return [];
        }
    }
}
