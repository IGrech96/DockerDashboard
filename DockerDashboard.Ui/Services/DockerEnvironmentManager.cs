using DockerDashboard.Shared.Services.Environment;

namespace DockerDashboard.Ui.Services
{
    public class DockerEnvironmentManager : IDockerEnvironmentManager
    {
        public async IAsyncEnumerable<DockerEnvironment> GetAllEnvironmentsAsync(CancellationToken cancellationToken)
        {
             yield return new DockerEnvironment() { Id = 1, Name ="Local" };
        }
    }
}
