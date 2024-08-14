using DockerDashboard.Shared.Data;

namespace DockerDashboard.Shared.Services.Environment;

public interface IDockerEnvironmentManager
{
    IAsyncEnumerable<DockerEnvironment> GetAllEnvironmentsAsync(CancellationToken cancellationToken);
}