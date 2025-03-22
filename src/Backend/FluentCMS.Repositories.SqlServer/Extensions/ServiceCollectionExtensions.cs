using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.SqlServer.Configuration;
using FluentCMS.Repositories.SqlServer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace FluentCMS.Repositories.SqlServer.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register SQL Server repositories.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQL Server repository services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration section containing SQL Server settings.</param>
    /// <param name="entityAssemblies">The assemblies containing entity types.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSqlServerRepositories(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] entityAssemblies)
    {
        services.Configure<SqlServerSettings>(configuration);
        
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<SqlServerSettings>>().Value);
        
        var settings = new SqlServerSettings();
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
            
            // Apply migrations if supported
            if (dbContext.Database.ProviderName?.Contains("SqlServer") == true)
            {
                dbContext.Database.Migrate();
            }
        }
        
        return services;
    }
    
    /// <summary>
    /// Adds SQL Server repository services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">A delegate to configure SQL Server settings.</param>
    /// <param name="entityAssemblies">The assemblies containing entity types.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSqlServerRepositories(
        this IServiceCollection services,
        Action<SqlServerSettings> configureOptions,
        params Assembly[] entityAssemblies)
    {
        services.Configure(configureOptions);
        
        var settings = new SqlServerSettings();
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
            
            // Apply migrations if supported
            if (dbContext.Database.ProviderName?.Contains("SqlServer") == true)
            {
                dbContext.Database.Migrate();
            }
        }
        
        return services;
    }
    
    /// <summary>
    /// Registers a SQL Server repository for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the repository to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSqlServerRepository<TEntity>(this IServiceCollection services)
        where TEntity : class, IBaseEntity
    {
        services.AddScoped<IEnhancedBaseEntityRepository<TEntity>, SqlServerEntityRepository<TEntity>>();
        // Also register for the basic interface for backward compatibility
        services.AddScoped<IBaseEntityRepository<TEntity>>(sp => 
            (IBaseEntityRepository<TEntity>)sp.GetRequiredService<IEnhancedBaseEntityRepository<TEntity>>());
        
        return services;
    }
    
    /// <summary>
    /// Scans the provided assembly for entity types and automatically registers SQL Server repositories for each one.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add repositories to.</param>
    /// <param name="entityAssembly">The assembly to scan for entity types.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSqlServerRepositoriesFromAssembly(
        this IServiceCollection services,
        Assembly entityAssembly)
    {
        var entityTypes = entityAssembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IBaseEntity).IsAssignableFrom(t));
            
        foreach (var entityType in entityTypes)
        {
            // Use reflection to create and invoke the generic AddSqlServerRepository method
            var method = typeof(ServiceCollectionExtensions)
                .GetMethod(nameof(AddSqlServerRepository))
                ?.MakeGenericMethod(entityType);
                
            method?.Invoke(null, new object[] { services });
        }
        
        return services;
    }
    
    /// <summary>
    /// Configures the DbContext options based on settings.
    /// </summary>
    /// <param name="options">The options builder to configure.</param>
    /// <param name="settings">The SQL Server settings.</param>
    private static void ConfigureDbContext(DbContextOptionsBuilder options, SqlServerSettings settings)
    {
        options.UseSqlServer(settings.ConnectionString, sqlServerOptions => 
        {
            sqlServerOptions.EnableRetryOnFailure(
                settings.MaxRetryCount,
                TimeSpan.FromSeconds(settings.MaxRetryDelay),
                null);
                
            if (settings.CommandTimeout > 0)
            {
                sqlServerOptions.CommandTimeout(settings.CommandTimeout);
            }
            
            // Configure migrations assembly
            sqlServerOptions.MigrationsAssembly(typeof(FluentCMSDbContext).Assembly.GetName().Name);
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
