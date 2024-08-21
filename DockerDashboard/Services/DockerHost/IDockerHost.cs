using System.Runtime.CompilerServices;
using DockerDashboard.Shared.Data;

namespace DockerDashboard.Services.DockerHost;

public interface IDockerHost
{
    Task StartWatchingAsync(CancellationToken cancellationToken);

    IAsyncEnumerable<ContainerLog> GetLogsAsync(string containerId, DateTimeOffset? since, DateTimeOffset? until, long? top, CancellationToken cancellationToken);
    IAsyncEnumerable<ContainerModel> GetContainers(string? beforeContainerId, long? take, CancellationToken cancellationToken);
    Task<ContainerModel?> GetContainer(string containerId, CancellationToken cancellationToken);
    Task<ContainerDetailedModel?> GetContainerDetails(string containerId, CancellationToken cancellationToken);
    Task PauseContainerAsync(string containerId, CancellationToken cancellationToken);
    Task StopContainerAsync(string containerId, CancellationToken cancellationToken);
    Task StartContainerAsync(string containerId, CancellationToken cancellationToken);
    Task DeleteContainerAsync(string containerId, CancellationToken cancellationToken);
    Task RestartContainerAsync(string containerId, CancellationToken cancellationToken);
    Task StopWatchingAsync(CancellationToken cancellationToken);
}