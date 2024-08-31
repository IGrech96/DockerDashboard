using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DockerDashboard.Shared;

public static class ServiceCollectionExtensions
{
    private static long _counter = 0;
    public delegate IDockerHost DockerHostFactory(IServiceProvider serviceProvider, DockerEnvironment dockerEnvironment);

    public static IServiceCollection AddEnvironment(this IServiceCollection services,  string name, DockerHostFactory hostFactory)
    {
        return services.AddSingleton(sp =>
        {
            var nextId = Interlocked.Increment(ref _counter);
            var environment = new DockerEnvironment() { Id = nextId, Name = name };

            Func<IDockerHost> factory = () => hostFactory(sp, environment);
            return new DockerEnvironmentProvider(environment, factory);

        });
    }
}

public class DockerEnvironmentProvider
{
    public DockerEnvironmentProvider(DockerEnvironment environment, Func<IDockerHost> hostFactory)
    {
        Environment = environment;
        HostFactory = hostFactory;
    }

    public DockerEnvironment Environment { get; }

    public Func<IDockerHost> HostFactory { get; }
}