using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkOptions(
        this IServiceCollection services, 
        Action<EntityFrameworkOptions> configure)
    {
        // Configure options
        services.Configure(configure);

        return services;
    }

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
