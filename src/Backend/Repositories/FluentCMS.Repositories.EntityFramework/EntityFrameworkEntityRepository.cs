using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.Abstractions.Querying;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Repositories.EntityFramework;

public class EntityFrameworkEntityRepository<TEntity> : IBaseEntityRepository<TEntity> where TEntity : class, IBaseEntity
{
    protected readonly FluentCmsDbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger<EntityFrameworkEntityRepository<TEntity>> _logger;

    public EntityFrameworkEntityRepository(
        FluentCmsDbContext dbContext,
        ILogger<EntityFrameworkEntityRepository<TEntity>> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = dbContext.Set<TEntity>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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

    public async Task<PagedResult<TEntity>> QueryAsync(
        QueryParameters<TEntity>? queryParameters = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = queryParameters ?? new QueryParameters<TEntity>();
        
        try
        {
            // Start with the base query
            IQueryable<TEntity> query = _dbSet;
            
            // Apply filter if provided
            if (parameters.FilterExpression != null)
            {
                query = query.Where(parameters.FilterExpression);
            }
            
            // Get total count before pagination for metadata
            var totalCount = await query.CountAsync(cancellationToken);
            
            // Apply sorting
            if (parameters.SortOptions.Any())
            {
                // We need to track if we've already applied the first sort
                bool isFirstSort = true;
                IOrderedQueryable<TEntity>? orderedQuery = null;
                
                foreach (var sortOption in parameters.SortOptions)
                {
                    // We need to get the property name and create a dynamic lambda
                    var propertyName = GetPropertyNameFromExpression(sortOption.KeySelector);
                    var parameter = Expression.Parameter(typeof(TEntity), "x");
                    var property = Expression.Property(parameter, propertyName);
                    var lambda = Expression.Lambda(property, parameter);
                    
                    // Get the generic method to call (OrderBy vs ThenBy and Ascending vs Descending)
                    var methodName = isFirstSort
                        ? (sortOption.Direction == SortDirection.Ascending ? "OrderBy" : "OrderByDescending")
                        : (sortOption.Direction == SortDirection.Ascending ? "ThenBy" : "ThenByDescending");
                    
                    // Get the property type
                    var propertyType = typeof(TEntity).GetProperty(propertyName)!.PropertyType;
                    
                    // Create the generic OrderBy/ThenBy method
                    var method = typeof(Queryable)
                        .GetMethods()
                        .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                        .MakeGenericMethod(typeof(TEntity), propertyType);
                    
                    // Apply the sorting
                    if (isFirstSort)
                    {
                        orderedQuery = (IOrderedQueryable<TEntity>)method.Invoke(null, [query, lambda])!;
                        isFirstSort = false;
                    }
                    else
                    {
                        orderedQuery = (IOrderedQueryable<TEntity>)method.Invoke(null, [orderedQuery, lambda])!;
                    }
                }
                
                // Use the ordered query if we applied sorting
                if (orderedQuery != null)
                {
                    query = orderedQuery;
                }
            }
            
            // Apply pagination
            query = query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize);
            
            // Execute query
            var items = await query.ToListAsync(cancellationToken);
            
            return new PagedResult<TEntity>(
                items,
                parameters.PageNumber,
                parameters.PageSize,
                totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying entities of type {EntityType}", typeof(TEntity).Name);
            return new PagedResult<TEntity>(
                Enumerable.Empty<TEntity>(), 
                parameters.PageNumber, 
                parameters.PageSize, 
                0);
        }
    }
    
    private string GetPropertyNameFromExpression(LambdaExpression expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        
        throw new ArgumentException("Expression must be a member access expression");
    }
}
