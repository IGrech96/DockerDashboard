using DockerDashboard.Shared;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Messaging;
using DockerDashboard.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DockerDashboard.Host.Demo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDemoEnvironment(this IServiceCollection services)
    {
        return services.AddEnvironment("Demo", CreateHostFactory);

        IDockerHost CreateHostFactory(IServiceProvider serviceprovider, DockerEnvironment dockerenvironment)
        {
            return new DemoDockerHost(serviceprovider.GetRequiredService<IMessageBus>(), dockerenvironment);
        }
    }
}
