using System.Runtime.CompilerServices;
using DockerDashboard.Shared.Data;

namespace DockerDashboard.Shared.Services;

public interface IDockerHost
{
    Task StartWatchingAsync(CancellationToken cancellationToken);
    Task StopWatchingAsync(CancellationToken cancellationToken);

    IDockerHostContainerManager ContainersHost { get; }

    IDockerHostImageManager ImagesHost { get; }
}