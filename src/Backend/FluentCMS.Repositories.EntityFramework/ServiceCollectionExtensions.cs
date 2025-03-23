using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.EntityFramework;

/// <summary>
/// Extension methods for configuring Entity Framework repositories with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Entity Framework repository options to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure the Entity Framework options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddEntityFrameworkOptions(
        this IServiceCollection services, 
        Action<EntityFrameworkOptions> configure)
    {
        // Configure options
        services.Configure(configure);

        return services;
    }

    /// <summary>
    /// Adds Entity Framework repository options to the service collection using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section containing Entity Framework settings.</param>
    /// <param name="sectionName">The name of the configuration section to use. Defaults to "EntityFramework".</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddEntityFrameworkOptions(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "EntityFramework")
    {
        // Configure options from configuration
        services.Configure<EntityFrameworkOptions>(options => 
        {
            // Get configuration section
            var section = configuration.GetSection(sectionName);
            
            // Manually transfer configuration values to options
            bool useCamelCaseValue;
            if (bool.TryParse(section["UseCamelCaseTableNames"], out useCamelCaseValue))
            {
                options.UseCamelCaseTableNames = useCamelCaseValue;
            }
            
            options.TableNamePrefix = section["TableNamePrefix"];
            options.TableNameSuffix = section["TableNameSuffix"];
            options.DefaultSchema = section["DefaultSchema"];
            
            bool autoMigrateValue;
            if (bool.TryParse(section["AutoMigrateDatabase"], out autoMigrateValue))
            {
                options.AutoMigrateDatabase = autoMigrateValue;
            }
            
            bool usePluralValue;
            if (bool.TryParse(section["UsePluralTableNames"], out usePluralValue))
            {
                options.UsePluralTableNames = usePluralValue;
            }
        });

        return services;
    }

    /// <summary>
    /// Registers Entity Framework repositories using the specified database provider.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext to use.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="dbContextOptionsBuilder">An action to configure the DbContextOptions.</param>
    /// <param name="configure">An optional action to configure Entity Framework options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddEntityFrameworkRepositories<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> dbContextOptionsBuilder,
        Action<EntityFrameworkOptions>? configure = null)
        where TContext : FluentCmsDbContext
    {
        // Configure options if provided
        if (configure != null)
        {
            services.AddEntityFrameworkOptions(configure);
        }

        // Register DbContext
        services.AddDbContext<TContext>(dbContextOptionsBuilder);
        
        // Register FluentCmsDbContext (if TContext is a derived type)
        if (typeof(TContext) != typeof(FluentCmsDbContext))
        {
            services.AddScoped<FluentCmsDbContext>(sp => sp.GetRequiredService<TContext>());
        }

        // Register generic repository
        services.AddScoped(typeof(IBaseEntityRepository<>), typeof(EntityFrameworkEntityRepository<>));

        return services;
    }

    /// <summary>
    /// Core method to register Entity Framework repositories with the FluentCmsDbContext.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="dbContextOptionsBuilder">An action to configure the DbContextOptions.</param>
    /// <param name="configure">An optional action to configure Entity Framework options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddEntityFrameworkRepositories(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> dbContextOptionsBuilder,
        Action<EntityFrameworkOptions>? configure = null)
    {
        return AddEntityFrameworkRepositories<FluentCmsDbContext>(
            services, 
            dbContextOptionsBuilder, 
            configure);
    }
}
