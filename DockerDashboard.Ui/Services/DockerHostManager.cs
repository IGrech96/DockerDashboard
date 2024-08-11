using DockerDashboard.Shared.Data;
using Simple.OData.Client;
using System.Runtime.CompilerServices;
using DockerDashboard.Ui.Clients;

namespace DockerDashboard.Ui.Services
{
    public class DockerHostManager : IDockerHostManager
    {
        private const string ContainersCollection = "Containers";
        private readonly ODataClient _client;

        public DockerHostManager([FromKeyedServices(ClientCategory.Backend)]ODataClient client)
        {
            _client = client;
        }

        public Task DeleteContainerAsync(long environment, string containerId)
        {
            throw new NotImplementedException();
        }

        public async Task<ContainerModel> GetContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            var data = await _client
                .For<ContainerModel>(ContainersCollection)
                .QueryOptions($"environment={environment}")
                .Filter(m => m.ContainerId == containerId)
                .FindEntryAsync(cancellationToken);

            return data;
        }

        public Task<ContainerDetailedModel> GetContainerDetails(long environment, string containerId)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<string> GetContainerLogsAsync(long environment, string containerId, DateTimeOffset since, DateTimeOffset until)
        {
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<ContainerModel> GetContainers(long environment, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var data = _client
                .For<ContainerModel>(ContainersCollection)
                .QueryOptions($"environment={environment}")
                .FindEntriesAllPagesAsync(cancellationToken);

            await foreach (var dockerEnvironment in data)
            {
                yield return dockerEnvironment;
            }
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
