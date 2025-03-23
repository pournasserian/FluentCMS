using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Repositories.Factory.Providers;

public interface IProviderConfigurator
{
    void ConfigureServices(IServiceCollection services, RepositoryFactoryOptions options);

    bool CanHandleProvider(string providerName);
    
    void ValidateConfiguration(RepositoryFactoryOptions options);
}
