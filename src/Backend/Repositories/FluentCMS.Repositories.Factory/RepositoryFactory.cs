using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.Abstractions.Querying;
using FluentCMS.Repositories.EntityFramework;
using FluentCMS.Repositories.LiteDB;
using FluentCMS.Repositories.MongoDB;
using FluentCMS.Repositories.PostgreSQL;
using FluentCMS.Repositories.SQLite;
using FluentCMS.Repositories.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.Factory;

public class RepositoryFactory<TEntity> : IBaseEntityRepository<TEntity> where TEntity : class, IBaseEntity
{
    private readonly IBaseEntityRepository<TEntity> _repository;

    public RepositoryFactory(
        IOptions<RepositoryFactoryOptions> options,
        IServiceProvider serviceProvider)
    {
        _repository = CreateRepository(options.Value, serviceProvider);
    }

    private IBaseEntityRepository<TEntity> CreateRepository(
        RepositoryFactoryOptions options,
        IServiceProvider serviceProvider)
    {
        switch (options.Provider.ToLowerInvariant())
        {
            case "mongodb":
                return ActivatorUtilities.CreateInstance<MongoDbEntityRepository<TEntity>>(
                    serviceProvider);

            case "litedb":
                return ActivatorUtilities.CreateInstance<LiteDbEntityRepository<TEntity>>(
                    serviceProvider);

            case "entityframework":
            case "ef":
                return ActivatorUtilities.CreateInstance<EntityFrameworkEntityRepository<TEntity>>(
                    serviceProvider);

            case "sqlite":
                // SQLite uses EntityFrameworkEntityRepository with SQLite-specific DbContext
                return ActivatorUtilities.CreateInstance<EntityFrameworkEntityRepository<TEntity>>(
                    serviceProvider);

            case "sqlserver":
                // SQL Server uses EntityFrameworkEntityRepository with SQL Server-specific DbContext
                return ActivatorUtilities.CreateInstance<EntityFrameworkEntityRepository<TEntity>>(
                    serviceProvider);
                    
            case "postgresql":
            case "postgres":
                // PostgreSQL uses EntityFrameworkEntityRepository with PostgreSQL-specific DbContext
                return ActivatorUtilities.CreateInstance<EntityFrameworkEntityRepository<TEntity>>(
                    serviceProvider);

            default:
                throw new NotSupportedException($"Repository provider '{options.Provider}' is not supported. " +
                    $"Valid providers are: MongoDB, LiteDB, EntityFramework, SQLite, SqlServer, PostgreSQL.");
        }
    }

    // IBaseEntityRepository implementation - delegating to the selected repository
    
    public Task<TEntity?> Create(TEntity entity, CancellationToken cancellationToken = default) 
        => _repository.Create(entity, cancellationToken);

    public Task<IEnumerable<TEntity>> CreateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) 
        => _repository.CreateMany(entities, cancellationToken);

    public Task<TEntity?> Update(TEntity entity, CancellationToken cancellationToken = default) 
        => _repository.Update(entity, cancellationToken);

    public Task<IEnumerable<TEntity>> UpdateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) 
        => _repository.UpdateMany(entities, cancellationToken);

    public Task<TEntity?> Delete(Guid id, CancellationToken cancellationToken = default) 
        => _repository.Delete(id, cancellationToken);

    public Task<IEnumerable<TEntity>> DeleteMany(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) 
        => _repository.DeleteMany(ids, cancellationToken);

    public Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default) 
        => _repository.GetAll(cancellationToken);

    public Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken = default) 
        => _repository.GetById(id, cancellationToken);

    public Task<IEnumerable<TEntity>> GetByIds(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) 
        => _repository.GetByIds(ids, cancellationToken);
        
    public Task<PagedResult<TEntity>> QueryAsync(QueryParameters<TEntity>? queryParameters = default, CancellationToken cancellationToken = default)
        => _repository.QueryAsync(queryParameters, cancellationToken);
}
