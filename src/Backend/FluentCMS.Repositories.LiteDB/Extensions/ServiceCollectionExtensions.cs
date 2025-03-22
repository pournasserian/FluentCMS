using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.LiteDB.Configuration;
using FluentCMS.Repositories.LiteDB.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.LiteDB.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register LiteDB repositories.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds LiteDB repository services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration section containing LiteDB settings.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddLiteDbRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LiteDbSettings>(configuration);
        
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<LiteDbSettings>>().Value);
        
        if (services.BuildServiceProvider().GetRequiredService<LiteDbSettings>().UseSharedConnection)
        {
            // Register as singleton for shared connection
            services.AddSingleton<LiteDbContext>();
        }
        else
        {
            // Register as scoped for independent connections per scope
            services.AddScoped<LiteDbContext>();
        }
        
        return services;
    }
    
    /// <summary>
    /// Adds LiteDB repository services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">A delegate to configure LiteDB settings.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddLiteDbRepositories(
        this IServiceCollection services,
        Action<LiteDbSettings> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<LiteDbSettings>>().Value);
        
        var settings = new LiteDbSettings();
        configureOptions(settings);
        
        if (settings.UseSharedConnection)
        {
            // Register as singleton for shared connection
            services.AddSingleton<LiteDbContext>();
        }
        else
        {
            // Register as scoped for independent connections per scope
            services.AddScoped<LiteDbContext>();
        }
        
        return services;
    }
    
    /// <summary>
    /// Registers a LiteDB repository for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the repository to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddLiteDbRepository<TEntity>(this IServiceCollection services)
        where TEntity : class, IBaseEntity
    {
        services.AddScoped<IEnhancedBaseEntityRepository<TEntity>, LiteDbEntityRepository<TEntity>>();
        // Also register for the basic interface for backward compatibility
        services.AddScoped<IBaseEntityRepository<TEntity>>(sp => 
            (IBaseEntityRepository<TEntity>)sp.GetRequiredService<IEnhancedBaseEntityRepository<TEntity>>());
        
        return services;
    }
    
    /// <summary>
    /// Scans the provided assembly for entity types and automatically registers LiteDB repositories for each one.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add repositories to.</param>
    /// <param name="entityAssembly">The assembly to scan for entity types.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddLiteDbRepositoriesFromAssembly(
        this IServiceCollection services,
        System.Reflection.Assembly entityAssembly)
    {
        var entityTypes = entityAssembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IBaseEntity).IsAssignableFrom(t));
            
        foreach (var entityType in entityTypes)
        {
            // Use reflection to create and invoke the generic AddLiteDbRepository method
            var method = typeof(ServiceCollectionExtensions)
                .GetMethod(nameof(AddLiteDbRepository))
                ?.MakeGenericMethod(entityType);
                
            method?.Invoke(null, new object[] { services });
        }
        
        return services;
    }
}
