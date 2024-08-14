using DockerDashboard.Shared.Data;
using Simple.OData.Client;
using System.Runtime.CompilerServices;
using DockerDashboard.Shared.Services;
using DockerDashboard.Ui.Clients;

namespace DockerDashboard.Ui.Services
{
    public class DockerHostManager : IDockerHostManager
    {
        private const string ContainersCollection = "Containers";
        private const string DetailsAction = "Details";

        private readonly ODataClient _client;

        public DockerHostManager([FromKeyedServices(ClientCategory.Backend)]ODataClient client)
        {
            _client = client;
        }

        public async Task<ContainerModel> GetContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            var data = await _client
                .For<ContainerModel>(ContainersCollection)
                .QueryOptions($"environment={environment}")
                .Key(containerId)
                .FindEntryAsync(cancellationToken);

                return data;
        }

        public async Task<ContainerDetailedModel> GetContainerDetails(long environment, string containerId, CancellationToken cancellationToken)
        {
            var data = await _client
                .For<ContainerModel>(ContainersCollection)
                //.QueryOptions($"environment={environment}")
                .Key(containerId)
                .Function<ContainerDetailedModel>(DetailsAction)
                .Set(new {environment})
                .ExecuteAsSingleAsync(cancellationToken);

            return data;
        }

        public IAsyncEnumerable<string> GetContainerLogsAsync(long environment, string containerId, DateTimeOffset since, DateTimeOffset until)
        {
            //TODO: deside how to use paging here
            return AsyncEnumerable.Empty<string>();
        }

        public async IAsyncEnumerable<ContainerModel> GetContainers(long environment, string? beforeContainerId, long? take, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            //TODO: handle take if passed
            var client = _client
                .For<ContainerModel>(ContainersCollection)
                .QueryOptions($"environment={environment}");

            if (beforeContainerId != null)
            {
                client = client.QueryOptions($"$beforeContainerId={beforeContainerId}");
            }

            var data = client.FindEntriesAllPagesAsync(cancellationToken);

            await foreach (var dockerEnvironment in data)
            {
                yield return dockerEnvironment;
            }
        }

        public async Task DeleteContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            await _client
                .For<ContainerModel>(ContainersCollection)
                .QueryOptions($"environment={environment}")
                .Key(containerId)
                .DeleteEntryAsync(cancellationToken);
        }

        public async Task PauseContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            await _client
                .For<ContainerModel>(ContainersCollection)
                //.QueryOptions($"environment={environment}")
                .Key(containerId)
                .Action("Pause")
                .Set(new {environment})
                .ExecuteAsync(cancellationToken);

        }

        public async Task RestartContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            await _client
                .For<ContainerModel>(ContainersCollection)
                //.QueryOptions($"environment={environment}")
                .Key(containerId)
                .Action("Restart")
                .Set(new {environment})
                .ExecuteAsync(cancellationToken);
        }

        public async Task StartContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            await _client
                .For<ContainerModel>(ContainersCollection)
                //.QueryOptions($"environment={environment}")
                .Key(containerId)
                .Action("Start")
                .Set(new {environment})
                .ExecuteAsync(cancellationToken);
        }

        public async Task StopContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            await _client
                .For<ContainerModel>(ContainersCollection)
                //.QueryOptions($"environment={environment}")
                .Key(containerId)
                .Action("Stop")
                .Set(new {environment})
                .ExecuteAsync(cancellationToken);
        }
    }
}
