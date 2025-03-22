using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.SQLite.Configuration;
using FluentCMS.Repositories.SQLite.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace FluentCMS.Repositories.SQLite.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register SQLite repositories.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQLite repository services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration section containing SQLite settings.</param>
    /// <param name="entityAssemblies">The assemblies containing entity types.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSQLiteRepositories(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] entityAssemblies)
    {
        services.Configure<SQLiteSettings>(configuration);
        
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<SQLiteSettings>>().Value);
        
        var settings = new SQLiteSettings();
        configuration.Bind(settings);
        settings.Validate();
        
        // Get entity types
        var entityTypes = GetEntityTypes(entityAssemblies);
        
        // Register DbContext
        if (settings.UseSharedContext)
        {
            services.AddDbContext<FluentCMSDbContext>(options =>
            {
                ConfigureDbContext(options, settings);
            }, ServiceLifetime.Singleton);
        }
        else
        {
            services.AddDbContext<FluentCMSDbContext>(options =>
            {
                ConfigureDbContext(options, settings);
            });
        }
        
        // Register entity types with the DbContext
        services.AddSingleton(entityTypes);
        
        // Auto-migrate the database if enabled
        if (settings.AutoMigrate)
        {
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FluentCMSDbContext>();
            dbContext.Database.Migrate();
        }
        
        return services;
    }
    
    /// <summary>
    /// Adds SQLite repository services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">A delegate to configure SQLite settings.</param>
    /// <param name="entityAssemblies">The assemblies containing entity types.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSQLiteRepositories(
        this IServiceCollection services,
        Action<SQLiteSettings> configureOptions,
        params Assembly[] entityAssemblies)
    {
        services.Configure(configureOptions);
        
        var settings = new SQLiteSettings();
        configureOptions(settings);
        settings.Validate();
        
        services.AddSingleton(settings);
        
        // Get entity types
        var entityTypes = GetEntityTypes(entityAssemblies);
        
        // Register DbContext
        if (settings.UseSharedContext)
        {
            services.AddDbContext<FluentCMSDbContext>(options =>
            {
                ConfigureDbContext(options, settings);
            }, ServiceLifetime.Singleton);
        }
        else
        {
            services.AddDbContext<FluentCMSDbContext>(options =>
            {
                ConfigureDbContext(options, settings);
            });
        }
        
        // Register entity types with the DbContext
        services.AddSingleton(entityTypes);
        
        // Auto-migrate the database if enabled
        if (settings.AutoMigrate)
        {
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FluentCMSDbContext>();
            dbContext.Database.Migrate();
        }
        
        return services;
    }
    
    /// <summary>
    /// Registers a SQLite repository for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the repository to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSQLiteRepository<TEntity>(this IServiceCollection services)
        where TEntity : class, IBaseEntity
    {
        services.AddScoped<IEnhancedBaseEntityRepository<TEntity>, SQLiteEntityRepository<TEntity>>();
        // Also register for the basic interface for backward compatibility
        services.AddScoped<IBaseEntityRepository<TEntity>>(sp => 
            (IBaseEntityRepository<TEntity>)sp.GetRequiredService<IEnhancedBaseEntityRepository<TEntity>>());
        
        return services;
    }
    
    /// <summary>
    /// Scans the provided assembly for entity types and automatically registers SQLite repositories for each one.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add repositories to.</param>
    /// <param name="entityAssembly">The assembly to scan for entity types.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSQLiteRepositoriesFromAssembly(
        this IServiceCollection services,
        Assembly entityAssembly)
    {
        var entityTypes = entityAssembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IBaseEntity).IsAssignableFrom(t));
            
        foreach (var entityType in entityTypes)
        {
            // Use reflection to create and invoke the generic AddSQLiteRepository method
            var method = typeof(ServiceCollectionExtensions)
                .GetMethod(nameof(AddSQLiteRepository))
                ?.MakeGenericMethod(entityType);
                
            method?.Invoke(null, new object[] { services });
        }
        
        return services;
    }
    
    /// <summary>
    /// Configures the DbContext options based on settings.
    /// </summary>
    /// <param name="options">The options builder to configure.</param>
    /// <param name="settings">The SQLite settings.</param>
    private static void ConfigureDbContext(DbContextOptionsBuilder options, SQLiteSettings settings)
    {
        options.UseSqlite(settings.ConnectionString, sqliteOptions =>
        {
            sqliteOptions.MigrationsAssembly(typeof(FluentCMSDbContext).Assembly.GetName().Name);
        });
        
        if (!settings.AutoDetectChanges)
        {
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
        
        if (settings.EnableDetailedErrors)
        {
            options.EnableDetailedErrors();
        }
        
        if (settings.EnableSensitiveDataLogging)
        {
            options.EnableSensitiveDataLogging();
        }
        
        if (settings.CommandTimeout > 0)
        {
            options.CommandTimeout(settings.CommandTimeout);
        }
        
        if (settings.MaxRetryCount > 0)
        {
            options.EnableRetryOnFailure(settings.MaxRetryCount);
        }
    }
    
    /// <summary>
    /// Gets entity types from the provided assemblies.
    /// </summary>
    /// <param name="entityAssemblies">The assemblies to scan for entity types.</param>
    /// <returns>A collection of entity types.</returns>
    private static IEnumerable<Type> GetEntityTypes(Assembly[] entityAssemblies)
    {
        var assemblies = entityAssemblies.Any() ? entityAssemblies : new[] { typeof(IBaseEntity).Assembly };
        
        return assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IBaseEntity).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .ToList();
    }
}
