using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.Abstractions.Querying;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace FluentCMS.Repositories.MongoDB;

/// <summary>
/// Repository implementation for MongoDB database provider.
/// </summary>
/// <typeparam name="TEntity">The entity type, which must implement IBaseEntity.</typeparam>
public class MongoDbEntityRepository<TEntity> : IBaseEntityRepository<TEntity> where TEntity : IBaseEntity
{
    private readonly IMongoCollection<TEntity> _collection;
    private readonly ILogger<MongoDbEntityRepository<TEntity>> _logger;

    // Static constructor to register conventions and mappings
    static MongoDbEntityRepository()
    {
        // Register convention pack to handle camelCase property mapping
        var conventionPack = new ConventionPack {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("CamelCase", conventionPack, _ => true);

        // Register Guid serialization
        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
        {
            BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(BsonType.String));
        }
    }

    /// <summary>
    /// Initializes a new instance of the MongoDbEntityRepository class.
    /// </summary>
    /// <param name="options">The MongoDB configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public MongoDbEntityRepository(
        IOptions<MongoDbOptions> options,
        ILogger<MongoDbEntityRepository<TEntity>> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);
        ArgumentException.ThrowIfNullOrEmpty(options.Value.ConnectionString);
        ArgumentException.ThrowIfNullOrEmpty(options.Value.DatabaseName);

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        try
        {
            // Configure MongoDB client and database
            var client = new MongoClient(options.Value.ConnectionString);
            var database = client.GetDatabase(options.Value.DatabaseName);

            // Get collection name with prefix/suffix and case conversion if configured
            var collectionName = GetCollectionName(typeof(TEntity).Name, options.Value);

            // Get MongoDB collection
            _collection = database.GetCollection<TEntity>(collectionName);

            // Create index on Id field
            var indexKeys = Builders<TEntity>.IndexKeys.Ascending(e => e.Id);
            var indexOptions = new CreateIndexOptions { Unique = true };
            _collection.Indexes.CreateOne(new CreateIndexModel<TEntity>(indexKeys, indexOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing MongoDB repository for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Gets the collection name based on configuration options.
    /// </summary>
    /// <param name="entityName">The entity name.</param>
    /// <param name="options">The MongoDB options.</param>
    /// <returns>The collection name.</returns>
    private static string GetCollectionName(string entityName, MongoDbOptions options)
    {
        string collectionName = entityName;

        // Apply camelCase if configured
        if (options.UseCamelCaseCollectionNames)
        {
            collectionName = char.ToLowerInvariant(collectionName[0]) + collectionName[1..];
        }

        // Apply prefix and suffix if configured
        if (!string.IsNullOrEmpty(options.CollectionNamePrefix))
        {
            collectionName = options.CollectionNamePrefix + collectionName;
        }

        if (!string.IsNullOrEmpty(options.CollectionNameSuffix))
        {
            collectionName = collectionName + options.CollectionNameSuffix;
        }

        return collectionName;
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

            // Insert the entity
            await _collection.InsertOneAsync(entity, null, cancellationToken);

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

            // Insert all entities
            await _collection.InsertManyAsync(entityList, null, cancellationToken);

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
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (entity.Id == Guid.Empty) throw new ArgumentException("Entity must have a valid ID to be updated.", nameof(entity));

        try
        {

            // Set update audit fields if entity is IAuditableEntity
            if (entity is IAuditableEntity auditableEntity)
            {
                auditableEntity.LastModifiedDate = DateTime.UtcNow;
            }

            // Filter for matching entity
            var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);

            // Update the entity
            var result = await _collection.ReplaceOneAsync(filter, entity,
                new ReplaceOptions { IsUpsert = false }, cancellationToken);

            // Return entity if update was successful
            return result.ModifiedCount > 0 ? entity : default;
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

            var updatedEntities = new List<TEntity>();
            foreach (var entity in entityList)
            {
                // Set update audit fields if entity is IAuditableEntity
                if (entity is IAuditableEntity auditableEntity)
                {
                    auditableEntity.LastModifiedDate = DateTime.UtcNow;
                }

                // Filter for matching entity
                var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);

                // Update the entity
                var result = await _collection.ReplaceOneAsync(filter, entity,
                    new ReplaceOptions { IsUpsert = false }, cancellationToken);

                // Add to updated entities if update was successful
                if (result.ModifiedCount > 0)
                {
                    updatedEntities.Add(entity);
                }
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
            var entity = await GetById(id, cancellationToken);
            if (entity == null) return default;

            // Filter for matching entity
            var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);

            // Delete the entity
            var result = await _collection.DeleteOneAsync(filter, cancellationToken);

            // Return entity if deletion was successful
            return result.DeletedCount > 0 ? entity : default;
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
            var entities = await GetByIds(idsList, cancellationToken);
            if (!entities.Any()) return [];

            // Filter for matching entities
            var filter = Builders<TEntity>.Filter.In(e => e.Id, idsList);

            // Delete the entities
            var result = await _collection.DeleteManyAsync(filter, cancellationToken);

            // Return entities if deletion was successful
            return result.DeletedCount > 0 ? entities : [];
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
            return await _collection.Find(Builders<TEntity>.Filter.Empty)
                .ToListAsync(cancellationToken);
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
            var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
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
            var filter = Builders<TEntity>.Filter.In(e => e.Id, idsList);
            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple entities of type {EntityType}",
                typeof(TEntity).Name);
            return [];
        }
    }

    /// <summary>
    /// Executes a MongoDB Find operation with the specified filter.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entities matching the filter.</returns>
    public async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding entities of type {EntityType} with filter",
                typeof(TEntity).Name);
            return [];
        }
    }

    /// <summary>
    /// Retrieves a paged list of entities based on the provided query parameters.
    /// </summary>
    /// <param name="queryParameters">The query parameters including filtering, sorting, and pagination options.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paged result containing the requested entities and pagination metadata.</returns>
    public async Task<PagedResult<TEntity>> QueryAsync(
        QueryParameters<TEntity>? queryParameters = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = queryParameters ?? new QueryParameters<TEntity>();
        
        try
        {
            // Build filter
            var filter = parameters.FilterExpression != null 
                ? Builders<TEntity>.Filter.Where(parameters.FilterExpression)
                : Builders<TEntity>.Filter.Empty;
                
            // Get total count
            var totalCount = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            
            // Build query
            var query = _collection.Find(filter);
            
            // Apply sorting
            if (parameters.SortOptions.Any())
            {
                var sortDefinitions = new List<SortDefinition<TEntity>>();
                
                foreach (var sortOption in parameters.SortOptions)
                {
                    if (sortOption.Direction == FluentCMS.Repositories.Abstractions.Querying.SortDirection.Ascending)
                        sortDefinitions.Add(Builders<TEntity>.Sort.Ascending(GetSortPropertyName(sortOption.KeySelector)));
                    else
                        sortDefinitions.Add(Builders<TEntity>.Sort.Descending(GetSortPropertyName(sortOption.KeySelector)));
                }
                
                if (sortDefinitions.Any())
                    query = query.Sort(Builders<TEntity>.Sort.Combine(sortDefinitions));
            }
            
            // Apply pagination
            var skip = (parameters.PageNumber - 1) * parameters.PageSize;
            query = query.Skip(skip).Limit(parameters.PageSize);
            
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
            return new PagedResult<TEntity>(Enumerable.Empty<TEntity>(), parameters.PageNumber, parameters.PageSize, 0);
        }
    }
    
    /// <summary>
    /// Extracts the property name from an expression.
    /// </summary>
    /// <param name="expression">The lambda expression.</param>
    /// <returns>The property name.</returns>
    private string GetSortPropertyName(LambdaExpression expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        
        throw new ArgumentException("Expression must be a member access expression");
    }
}
