using DockerDashboard.Shared.Data;

namespace DockerDashboard.Shared.Services;

public interface IDockerHostManager
{
    IAsyncEnumerable<ContainerModel> GetContainers(long environment, string? beforeContainerId, long? take, CancellationToken cancellationToken);

    Task<ContainerModel> GetContainerAsync(long environment, string containerId, CancellationToken cancellationToken);

    IAsyncEnumerable<string> GetContainerLogsAsync(long environment, string containerId, DateTimeOffset since, DateTimeOffset until);

    Task<ContainerDetailedModel> GetContainerDetails(long environment, string containerId, CancellationToken cancellationToken);

    Task PauseContainerAsync(long environment, string containerId, CancellationToken cancellationToken);

    Task StopContainerAsync(long environment, string containerId, CancellationToken cancellationToken);

    Task StartContainerAsync(long environment, string containerId, CancellationToken cancellationToken);

    Task DeleteContainerAsync(long environment, string containerId, CancellationToken cancellationToken);

    Task RestartContainerAsync(long environment, string containerId, CancellationToken cancellationToken);
}