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

    public IAsyncEnumerable<ContainerModel> GetContainers(long environment)
    {
        var host = dockerEnvironmentManager_.GetHost(environment);

        return host.GetContainers(CancellationToken.None);
    }

}