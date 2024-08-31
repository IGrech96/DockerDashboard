using DockerDashboard.Services.Environment;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services;

namespace DockerDashboard.Services.DockerHost;

public class DockerHostManager : IDockerHostManager
{
    private readonly DockerEnvironmentManager _dockerEnvironmentManager;

    public IDockerHostContainerManager GetContainerManager(long environment)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);
        return host.ContainersHost;
    }

    public DockerHostManager(DockerEnvironmentManager dockerEnvironmentManager)
    {
        _dockerEnvironmentManager = dockerEnvironmentManager;
    }
}