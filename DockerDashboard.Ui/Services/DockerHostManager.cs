using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services.DockerHost;

namespace DockerDashboard.Ui.Services
{
    public class DockerHostManager : IDockerHostManager
    {
        public Task DeleteContainerAsync(long environment, string containerId)
        {
            throw new NotImplementedException();
        }

        public Task<ContainerModel> GetContainerAsync(long environment, string containerId)
        {
            throw new NotImplementedException();
        }

        public Task<ContainerDetailedModel> GetContainerDetails(long environment, string containerId)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<string> GetContainerLogsAsync(long environment, string containerId, DateTimeOffset since, DateTimeOffset until)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<ContainerModel> GetContainers(long environment)
        {
            throw new NotImplementedException();
        }

        public Task PauseContainerAsync(long environment, string containerId)
        {
            throw new NotImplementedException();
        }

        public Task RestartContainerAsync(long environment, string containerId)
        {
            throw new NotImplementedException();
        }

        public Task StartContainerAsync(long environment, string containerId)
        {
            throw new NotImplementedException();
        }

        public Task StopContainerAsync(long environment, string containerId)
        {
            throw new NotImplementedException();
        }
    }
}
