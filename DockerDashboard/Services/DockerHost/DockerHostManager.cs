using DockerDashboard.Services.Environment;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services;

namespace DockerDashboard.Services.DockerHost;

public class DockerHostManager : IDockerHostManager
{
    private readonly DockerEnvironmentManager _dockerEnvironmentManager;

    public DockerHostManager(DockerEnvironmentManager dockerEnvironmentManager)
    {
        _dockerEnvironmentManager = dockerEnvironmentManager;
    }

    public Task<ContainerModel?> TryGetContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
    {
         var host = _dockerEnvironmentManager.GetHost(environment);

        return host.GetContainer(containerId, cancellationToken);
    }

    public IAsyncEnumerable<ContainerModel> GetContainers(long environment, string? beforeContainerId, long? take, CancellationToken cancellationToken)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.GetContainers(beforeContainerId, take, cancellationToken);
    }

    public IAsyncEnumerable<ContainerLog> GetContainerLogsAsync(long environment,
        string containerId,
        DateTimeOffset? since,
        DateTimeOffset? until, long? top, CancellationToken cancellationToken)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.GetLogsAsync(containerId, since, until, top, cancellationToken);
    }

    public Task<ContainerDetailedModel?> TryGetContainerDetails(long environment, string containerId, CancellationToken cancellationToken)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.GetContainerDetails(containerId, cancellationToken);
    }

    public Task PauseContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.PauseContainerAsync(containerId, cancellationToken);
    }

    public Task StopContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.StopContainerAsync(containerId, cancellationToken);
    }

    public Task StartContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.StartContainerAsync(containerId, cancellationToken);
    }

    public Task DeleteContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.DeleteContainerAsync(containerId, cancellationToken);
    }

    public Task RestartContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
    {
        var host = _dockerEnvironmentManager.GetHost(environment);

        return host.RestartContainerAsync(containerId, cancellationToken);
    }
}