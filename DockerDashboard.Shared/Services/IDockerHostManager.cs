namespace DockerDashboard.Shared.Services;

public interface IDockerHostManager
{
    IDockerHostContainerManager GetContainerManager(long environment);

    IDockerHostImageManager GetImageManager(long environment);
}