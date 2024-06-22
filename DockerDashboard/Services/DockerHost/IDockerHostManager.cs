using DockerDashboard.Data;

namespace DockerDashboard.Services.DockerHost;

public interface IDockerHostManager
{
    IAsyncEnumerable<ContainerModel> GetContainers(long environment);
}