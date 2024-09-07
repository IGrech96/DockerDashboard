using DockerDashboard.Shared;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Messaging;
using DockerDashboard.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DockerDashboard.Host.Docker;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalEnvironment(this IServiceCollection services)
    {
        return services.AddEnvironment("Local", CreateHostFactory);

        IDockerHost CreateHostFactory(IServiceProvider serviceprovider, DockerEnvironment dockerenvironment)
        {
            return new DockerHost(
                serviceprovider.GetRequiredService<IMessageBus>(),
                serviceprovider.GetRequiredService<IDockerRegistryManager>(),
                dockerenvironment);
        }
    }
}
