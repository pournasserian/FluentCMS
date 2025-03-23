using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

/// <summary>
/// Interface for provider configurators that set up provider-specific services.
/// </summary>
public interface IProviderConfigurator
{
    /// <summary>
    /// Configures the provider services in the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="options">The repository factory options.</param>
    void ConfigureServices(IServiceCollection services, RepositoryFactoryOptions options);

    /// <summary>
    /// Gets a value indicating whether this configurator can handle the specified provider.
    /// </summary>
    /// <param name="providerName">The provider name.</param>
    /// <returns>True if this configurator can handle the provider; otherwise, false.</returns>
    bool CanHandleProvider(string providerName);
    
    /// <summary>
    /// Validates the provider configuration.
    /// </summary>
    /// <param name="options">The repository factory options.</param>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
    void ValidateConfiguration(RepositoryFactoryOptions options);
}
