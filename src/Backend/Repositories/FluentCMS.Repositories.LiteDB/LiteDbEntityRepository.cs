using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.Abstractions.Querying;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace FluentCMS.Repositories.LiteDB;

public class LiteDbEntityRepository<TEntity> : IBaseEntityRepository<TEntity> where TEntity : IBaseEntity
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<TEntity> _collection;
    private readonly ILogger<LiteDbEntityRepository<TEntity>> _logger;

    public LiteDbEntityRepository(IOptions<LiteDbOptions> options, ILogger<LiteDbEntityRepository<TEntity>> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value.ConnectionString);

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        try
        {
            _database = new LiteDatabase(options.Value.ConnectionString);
            _collection = _database.GetCollection<TEntity>(typeof(TEntity).Name);

            // Configure collection
            _collection.EnsureIndex(x => x.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing LiteDB repository for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public async Task<TEntity?> Create(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

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

            // Insert the entity and return it if successful
            return await Task.Run(() => _collection.Insert(entity) != null ? entity : default, cancellationToken);
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
            // Ensure all entities have IDs
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

            // Insert all entities
            await Task.Run(() => _collection.InsertBulk(entityList), cancellationToken);
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
            // Set update audit fields if entity is IAuditableEntity
            if (entity is IAuditableEntity auditableEntity)
            {
                auditableEntity.LastModifiedDate = DateTime.UtcNow;
            }

            // Update the entity and return it if successful
            return await Task.Run(() => _collection.Update(entity) ? entity : default, cancellationToken);
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
            // Update each entity
            var updatedEntities = new List<TEntity>();
            await Task.Run(() =>
            {
                foreach (var entity in entityList)
                {
                    // Set update audit fields if entity is IAuditableEntity
                    if (entity is IAuditableEntity auditableEntity)
                    {
                        auditableEntity.LastModifiedDate = DateTime.UtcNow;
                    }

                    if (_collection.Update(entity))
                    {
                        updatedEntities.Add(entity);
                    }
                }
            }, cancellationToken);

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
            // Get the entity before deleting it
            var entity = await Task.Run(() => _collection.FindById(id), cancellationToken);
            if (entity == null) return default;

            // Delete the entity and return it if successful
            var isDeleted = await Task.Run(() => _collection.Delete(id), cancellationToken);
            return isDeleted ? entity : default;
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
            // Get all entities before deleting them
            var entities = await Task.Run(() => _collection.Find(x => idsList.Contains(x.Id)).ToList(), cancellationToken);
            if (entities.Count == 0) return [];

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
            return await Task.Run(() => _collection.FindAll(), cancellationToken);
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
            return await Task.Run(() => _collection.FindById(id), cancellationToken);
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
            return await Task.Run(() => _collection.Find(x => idsList.Contains(x.Id)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple entities of type {EntityType}",
                typeof(TEntity).Name);
            return [];
        }
    }

    public async Task<PagedResult<TEntity>> Query(QueryParameters<TEntity>? queryParameters = null, CancellationToken cancellationToken = default)
    {
        var parameters = queryParameters ?? new QueryParameters<TEntity>();

        try
        {
            return await Task.Run(() =>
            {
                IEnumerable<TEntity> results;

                // Apply filter if provided
                if (parameters.FilterExpression != null)
                {
                    results = _collection.Find(parameters.FilterExpression);
                }
                else
                {
                    results = _collection.FindAll();
                }

                // Get total count before pagination
                var totalCount = results.Count();

                // Apply sorting
                if (parameters.SortOptions.Any())
                {
                    // We need to apply sorting in-memory since we already executed the query
                    var firstSortOption = parameters.SortOptions.First();
                    var propertyInfo = typeof(TEntity).GetProperty(LiteDbEntityRepository<TEntity>.GetPropertyName(firstSortOption.KeySelector));

                    if (propertyInfo != null)
                    {
                        if (firstSortOption.Direction == SortDirection.Ascending)
                        {
                            results = results.OrderBy(e => propertyInfo.GetValue(e));
                        }
                        else
                        {
                            results = results.OrderByDescending(e => propertyInfo.GetValue(e));
                        }

                        // Apply additional sort options
                        foreach (var sortOption in parameters.SortOptions.Skip(1))
                        {
                            propertyInfo = typeof(TEntity).GetProperty(LiteDbEntityRepository<TEntity>.GetPropertyName(sortOption.KeySelector));
                            if (propertyInfo != null)
                            {
                                var orderedResults = results as IOrderedEnumerable<TEntity>;
                                if (sortOption.Direction == SortDirection.Ascending)
                                {
                                    results = orderedResults!.ThenBy(e => propertyInfo.GetValue(e));
                                }
                                else
                                {
                                    results = orderedResults!.ThenByDescending(e => propertyInfo.GetValue(e));
                                }
                            }
                        }
                    }
                }

                // Apply pagination
                var items = results
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToList();

                return new PagedResult<TEntity>(
                    items,
                    parameters.PageNumber,
                    parameters.PageSize,
                    totalCount);
            }, cancellationToken);
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

    private static string GetPropertyName(LambdaExpression expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new ArgumentException("Expression must be a member access expression");
    }

    ~LiteDbEntityRepository()
    {
        _database?.Dispose();
    }
}
