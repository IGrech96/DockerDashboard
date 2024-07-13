using DockerDashboard.Services.Environment;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services.DockerHost;

namespace DockerDashboard.Services.DockerHost;

public class DockerHostManager : IDockerHostManager
{
    private readonly DockerEnvironmentManager _dockerEnvironmentManager;

    public DockerHostManager(DockerEnvironmentManager dockerEnvironmentManager)
    {
        _dockerEnvironmentManager = dockerEnvironmentManager;
    }

    public Task<ContainerModel> GetContainerAsync(long environment, string containerId)
    {
         var host = _dockerEnvironmentManager.GetHost(environment);

        return host.GetContainer(containerId, CancellationToken.None);
    }

    public IAsyncEnumerable<ContainerModel> GetContainers(long environment)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.GetContainers(CancellationToken.None);
    }

    public IAsyncEnumerable<string> GetContainerLogsAsync(long environment, string containerId, DateTimeOffset since,
        DateTimeOffset until)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.GetLogsAsync(containerId, since, until, CancellationToken.None);
    }

    public Task<ContainerDetailedModel> GetContainerDetails(long environment, string containerId)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.GetContainerDetails(containerId, CancellationToken.None);
    }

    public Task PauseContainerAsync(long environment, string containerId)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.PauseContainerAsync(containerId, CancellationToken.None);
    }

    public Task StopContainerAsync(long environment, string containerId)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.StopContainerAsync(containerId, CancellationToken.None);
    }

    public Task StartContainerAsync(long environment, string containerId)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.StartContainerAsync(containerId, CancellationToken.None);
    }

    public Task DeleteContainerAsync(long environment, string containerId)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.DeleteContainerAsync(containerId, CancellationToken.None);
    }

    public Task RestartContainerAsync(long environment, string containerId)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.RestartContainerAsync(containerId, CancellationToken.None);
    }
}