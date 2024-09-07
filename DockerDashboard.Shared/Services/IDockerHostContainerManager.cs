using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;

namespace DockerDashboard.Shared.Services;

public interface IDockerHostContainerManager
{
    IAsyncEnumerable<ContainerModel> GetContainersAsync(string? beforeContainerId, long? take, CancellationToken cancellationToken);

    Task<ContainerModel?> TryGetContainerAsync(string containerId, CancellationToken cancellationToken);

    IAsyncEnumerable<ContainerLog> GetContainerLogsAsync(string containerId, DateTimeOffset? since, DateTimeOffset? until, long? top, CancellationToken cancellationToken);

    Task<ContainerDetailedModel?> TryGetContainerDetailsAsync(string containerId, CancellationToken cancellationToken);

    Task PauseContainerAsync(string containerId, CancellationToken cancellationToken);

    Task StopContainerAsync(string containerId, CancellationToken cancellationToken);

    Task StartContainerAsync(string containerId, CancellationToken cancellationToken);

    Task DeleteContainerAsync(string containerId, CancellationToken cancellationToken);

    Task RestartContainerAsync(string containerId, CancellationToken cancellationToken);

    Task RecreateContainerAsync(string containerId, bool pullImage, IProgress<ProgressEvent> progress, CancellationToken cancellationToken);
}