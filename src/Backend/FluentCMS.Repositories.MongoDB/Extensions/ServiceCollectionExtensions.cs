using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.MongoDB.Configuration;
using FluentCMS.Repositories.MongoDB.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.MongoDB.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register MongoDB repositories.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MongoDB repository services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration section containing MongoDB settings.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMongoDbRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration);
        
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);
        services.AddSingleton<MongoDbContext>();
        
        return services;
    }
    
    /// <summary>
    /// Adds MongoDB repository services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">A delegate to configure MongoDB settings.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMongoDbRepositories(
        this IServiceCollection services,
        Action<MongoDbSettings> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);
        services.AddSingleton<MongoDbContext>();
        
        return services;
    }
    
    /// <summary>
    /// Registers a MongoDB repository for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the repository to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMongoDbRepository<TEntity>(this IServiceCollection services)
        where TEntity : class, IBaseEntity
    {
        services.AddScoped<IEnhancedBaseEntityRepository<TEntity>, MongoDbEntityRepository<TEntity>>();
        // Also register for the basic interface for backward compatibility
        services.AddScoped<IBaseEntityRepository<TEntity>>(sp => 
            (IBaseEntityRepository<TEntity>)sp.GetRequiredService<IEnhancedBaseEntityRepository<TEntity>>());
        
        return services;
    }
    
    /// <summary>
    /// Scans the provided assembly for entity types and automatically registers MongoDB repositories for each one.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add repositories to.</param>
    /// <param name="entityAssembly">The assembly to scan for entity types.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMongoDbRepositoriesFromAssembly(
        this IServiceCollection services,
        System.Reflection.Assembly entityAssembly)
    {
        var entityTypes = entityAssembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IBaseEntity).IsAssignableFrom(t));
            
        foreach (var entityType in entityTypes)
        {
            // Use reflection to create and invoke the generic AddMongoDbRepository method
            var method = typeof(ServiceCollectionExtensions)
                .GetMethod(nameof(AddMongoDbRepository))
                ?.MakeGenericMethod(entityType);
                
            method?.Invoke(null, new object[] { services });
        }
        
        return services;
    }
}
