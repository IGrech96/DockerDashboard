using DockerDashboard.Services.Environment;
using DockerDashboard.Data;

namespace DockerDashboard.Services.DockerHost;

public class DockerHostManager : IDockerHostManager
{
    private readonly DockerEnvironmentManager dockerEnvironmentManager_;

    public DockerHostManager(DockerEnvironmentManager dockerEnvironmentManager)
    {
        dockerEnvironmentManager_ = dockerEnvironmentManager;
    }

    public Task<ContainerModel> GetContainerAsync(long environment, string containerId)
    {
         var host = dockerEnvironmentManager_.GetHost(environment);

        return host.GetContainer(containerId, CancellationToken.None);
    }

    public IAsyncEnumerable<ContainerModel> GetContainers(long environment)
    {
        var host = dockerEnvironmentManager_.GetHost(environment);

        return host.GetContainers(CancellationToken.None);
    }

    public Task<string[]> GetContainerLogsAsync(long environment, string containerId, DateTimeOffset since, DateTimeOffset until)
    {
        var host = dockerEnvironmentManager_.GetHost(environment);

        return host.GetLogsAsync(containerId, since, until, CancellationToken.None);
    }
}