using DockerDashboard.Shared.Data;

namespace DockerDashboard.Shared.Services;

public interface IDockerRegistryManager
{
    public Task<DockerRegistry?> TryGetRegistryAsync(string imageName, CancellationToken cancellationToken);

    public Task<(string username, string password)?> TryGetCredentailsAsync(DockerRegistry registry, CancellationToken cancellationToken);
}