using FluentCMS.Entities;
using FluentCMS.Repositories.LiteDB.Configuration;
using LiteDB;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace FluentCMS.Repositories.LiteDB.Infrastructure;

/// <summary>
/// LiteDB database context for FluentCMS entities.
/// </summary>
public class LiteDbContext : IDisposable
{
    private readonly LiteDbSettings _settings;
    private readonly LiteDatabase _database;
    private readonly ConcurrentDictionary<Type, object> _collections = new();
    private bool _isDisposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteDbContext"/> class.
    /// </summary>
    /// <param name="settings">The LiteDB settings.</param>
    public LiteDbContext(LiteDbSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _settings.Validate();

        var connectionString = new ConnectionString(_settings.ConnectionString);
        
        if (_settings.InitialSize > 0)
        {
            connectionString.InitialSize = _settings.InitialSize * 1024 * 1024; // Convert to bytes
        }
        
        if (_settings.UpgradeDatabase)
        {
            connectionString.Upgrade = true;
        }

        // Configure and open LiteDB database
        _database = new LiteDatabase(connectionString);
        
        // Register custom types and mappings
        RegisterMappings();
    }

    /// <summary>
    /// Gets the underlying LiteDatabase instance.
    /// </summary>
    public LiteDatabase Database => _database;

    /// <summary>
    /// Gets a LiteDB collection for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A LiteDB collection for the entity type.</returns>
    public ILiteCollection<TEntity> GetCollection<TEntity>() where TEntity : IBaseEntity
    {
        var entityType = typeof(TEntity);

        if (_collections.TryGetValue(entityType, out var existingCollection))
        {
            return (ILiteCollection<TEntity>)existingCollection;
        }

        var collectionName = GetCollectionName<TEntity>();
        var collection = _database.GetCollection<TEntity>(collectionName);

        // Create index on Id if auto-indexing is enabled
        if (_settings.AutoGenerateIndexes)
        {
            collection.EnsureIndex(x => x.Id);
        }

        // Apply soft delete filter if applicable
        if (_settings.ApplySoftDeleteFilter && typeof(ISoftDeleteBaseEntity).IsAssignableFrom(entityType))
        {
            var filter = GetSoftDeleteQueryFilter<TEntity>();
            collection = collection.Query().Where(filter).ToCollection();
        }

        _collections[entityType] = collection;
        return collection;
    }

    /// <summary>
    /// Gets a LiteDB collection for the specified entity type with an option to include soft-deleted entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="includeSoftDeleted">Whether to include soft-deleted entities.</param>
    /// <returns>A LiteDB collection for the entity type.</returns>
    public ILiteCollection<TEntity> GetCollection<TEntity>(bool includeSoftDeleted) where TEntity : IBaseEntity
    {
        if (includeSoftDeleted || !_settings.ApplySoftDeleteFilter || !typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
        {
            var collectionName = GetCollectionName<TEntity>();
            var collection = _database.GetCollection<TEntity>(collectionName);
            
            // Create index on Id if auto-indexing is enabled
            if (_settings.AutoGenerateIndexes)
            {
                collection.EnsureIndex(x => x.Id);
            }
            
            return collection;
        }

        return GetCollection<TEntity>();
    }

    /// <summary>
    /// Begins a LiteDB transaction.
    /// </summary>
    /// <returns>True if the transaction was started successfully; otherwise, false.</returns>
    public bool BeginTransaction() => _database.BeginTrans();

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <returns>True if the transaction was committed successfully; otherwise, false.</returns>
    public bool CommitTransaction() => _database.Commit();

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <returns>True if the transaction was rolled back successfully; otherwise, false.</returns>
    public bool RollbackTransaction() => _database.Rollback();

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

    private Expression<Func<TEntity, bool>> GetSoftDeleteQueryFilter<TEntity>() where TEntity : IBaseEntity
    {
        // Create a filter that checks if IsDeleted is false or doesn't exist
        if (typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
        {
            return e => ((ISoftDeleteBaseEntity)e).IsDeleted == false;
        }
        
        // If not a soft-delete entity, return a filter that matches everything
        return e => true;
    }

    private void RegisterMappings()
    {
        // Register custom type mappings if needed
        BsonMapper.Global.RegisterType<DateTime>(
            serialize: dt => dt.ToUniversalTime().ToString("o"),
            deserialize: bson => DateTime.Parse(bson.AsString).ToUniversalTime());

        BsonMapper.Global.RegisterType<DateTime?>(
            serialize: dt => dt.HasValue ? dt.Value.ToUniversalTime().ToString("o") : null,
            deserialize: bson => bson.IsNull ? null : DateTime.Parse(bson.AsString).ToUniversalTime());

        // Add more custom type mappings as needed
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">True if the method has been called directly or indirectly by user code.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _database?.Dispose();
            }

            _collections.Clear();
            _isDisposed = true;
        }
    }
}
