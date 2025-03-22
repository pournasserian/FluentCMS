using FluentCMS.Entities;
using FluentCMS.Repositories.MongoDB.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentCMS.Repositories.MongoDB.Infrastructure;

/// <summary>
/// MongoDB database context for FluentCMS entities.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;
    private readonly Dictionary<Type, object> _collections = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDbContext"/> class.
    /// </summary>
    /// <param name="settings">The MongoDB settings.</param>
    public MongoDbContext(MongoDbSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _settings.Validate();

        // Configure MongoDB conventions
        RegisterConventions();

        // Create MongoDB client and get database
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    /// <summary>
    /// Gets a MongoDB collection for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A MongoDB collection for the entity type.</returns>
    public IMongoCollection<TEntity> GetCollection<TEntity>() where TEntity : IBaseEntity
    {
        var entityType = typeof(TEntity);

        if (_collections.ContainsKey(entityType))
        {
            return (IMongoCollection<TEntity>)_collections[entityType];
        }

        var collectionName = GetCollectionName<TEntity>();
        var collection = _database.GetCollection<TEntity>(collectionName);

        // Apply soft delete filter if applicable
        if (_settings.ApplySoftDeleteFilter && typeof(ISoftDeleteBaseEntity).IsAssignableFrom(entityType))
        {
            var parameter = Expression.Parameter(entityType, "e");
            var isDeletedProperty = Expression.Property(parameter, "IsDeleted");
            var isFalseExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(isFalseExpression, parameter);

            var filter = Builders<TEntity>.Filter.Where(lambda);
            collection = collection.FindAll().Match(filter);
        }

        _collections[entityType] = collection;
        return collection;
    }

    /// <summary>
    /// Gets a MongoDB collection for the specified entity type with an option to include soft-deleted entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="includeSoftDeleted">Whether to include soft-deleted entities.</param>
    /// <returns>A MongoDB collection for the entity type.</returns>
    public IMongoCollection<TEntity> GetCollection<TEntity>(bool includeSoftDeleted) where TEntity : IBaseEntity
    {
        if (includeSoftDeleted || !_settings.ApplySoftDeleteFilter || !typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
        {
            var collectionName = GetCollectionName<TEntity>();
            return _database.GetCollection<TEntity>(collectionName);
        }

        return GetCollection<TEntity>();
    }

    /// <summary>
    /// Starts a session for transaction support.
    /// </summary>
    /// <returns>A client session handle.</returns>
    public IClientSessionHandle StartSession() => _database.Client.StartSession();

    private string GetCollectionName<TEntity>()
    {
        string name = typeof(TEntity).Name;

        // Remove "Entity" suffix if present
        if (name.EndsWith("Entity"))
        {
            name = name.Substring(0, name.Length - 6);
        }

        // Pluralize if configured
        if (_settings.PluralizeCollectionNames)
        {
            name = Pluralize(name);
        }

        // Add prefix if configured
        if (!string.IsNullOrEmpty(_settings.CollectionNamePrefix))
        {
            name = $"{_settings.CollectionNamePrefix}{name}";
        }

        return name;
    }

    private string Pluralize(string name)
    {
        // Simple pluralization rules - could be replaced with a more sophisticated library
        if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase))
        {
            return name.Substring(0, name.Length - 1) + "ies";
        }
        else if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                 name.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                 name.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                 name.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                 name.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
        {
            return name + "es";
        }
        else
        {
            return name + "s";
        }
    }

    private static void RegisterConventions()
    {
        // Register global conventions if not already registered
        if (!BsonClassMap.IsClassMapRegistered(typeof(IBaseEntity)))
        {
            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String)
            };

            ConventionRegistry.Register("FluentCMS Conventions", pack, _ => true);

            // Map the Id property to "_id" in MongoDB 
            BsonClassMap.RegisterClassMap<IBaseEntity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(c => c.Id)
                  .SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(BsonType.String));
            });

            // Register other entity mappings as needed
        }
    }
}
