using DockerDashboard.Shared.Data;
using Simple.OData.Client;
using System.Runtime.CompilerServices;
using DockerDashboard.Shared.Services;
using DockerDashboard.Ui.Clients;
using static DockerDashboard.Ui.Logging.Events;

namespace DockerDashboard.Ui.Services
{
    public class DockerHostManager : IDockerHostManager
    {
        private const string ContainersCollection = "Containers";
        private const string DetailsAction = "Details";

        private readonly ODataClient _client;
        private readonly ILogger<UserMarker> _logger;

        public DockerHostManager(
            [FromKeyedServices(ClientCategory.Backend)]ODataClient client,
            ILogger<UserMarker> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<ContainerModel?> TryGetContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            try
            {
                var data = await _client
                    .For<ContainerModel>(ContainersCollection)
                    .QueryOptions($"environment={environment}")
                    .Key(containerId)
                    .FindEntryAsync(cancellationToken);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(Logging.Events.Containers, ex, "Failed to find container '{containerId}'", containerId);
                return null;
            }
        }

        public async Task<ContainerDetailedModel?> TryGetContainerDetails(long environment, string containerId, CancellationToken cancellationToken)
        {
            try
            {
                var data = await _client
                    .For<ContainerModel>(ContainersCollection)
                    //.QueryOptions($"environment={environment}")
                    .Key(containerId)
                    .Function<ContainerDetailedModel>(DetailsAction)
                    .Set(new { environment })
                    .ExecuteAsSingleAsync(cancellationToken);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(Logging.Events.Containers, ex, "Failed to find container details '{containerId}'", containerId);
                return null;
            }
        }

        public async IAsyncEnumerable<ContainerLog> GetContainerLogsAsync(long environment, string containerId, DateTimeOffset? since, DateTimeOffset? until, long? top, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var client = _client
                .For<ContainerModel>(ContainersCollection);

            if (top.HasValue)
            {
                client = client.Top(top.Value);
            }

            var data = await client
                .Key(containerId)
                .Function<ContainerLog>("Logs")
                .Set(new {environment, until, since})
                .ExecuteAsArrayAsync(cancellationToken);

            foreach (var item in data)
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<ContainerModel> GetContainers(long environment, string? beforeContainerId, long? take, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            //TODO: error handling
            var client = _client
                .For<ContainerModel>(ContainersCollection)
                .QueryOptions($"environment={environment}");

            if (take != null)
            {
                client = client.Top(take.Value);
            }

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
            try
            {
                await _client
                    .For<ContainerModel>(ContainersCollection)
                    .QueryOptions($"environment={environment}")
                    .Key(containerId)
                    .DeleteEntryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(Logging.Events.Containers, ex, "Failed to delete '{containerId}'", containerId);
            }
        }

        public async Task PauseContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            try
            {
                await _client
                    .For<ContainerModel>(ContainersCollection)
                    //.QueryOptions($"environment={environment}")
                    .Key(containerId)
                    .Action("Pause")
                    .Set(new { environment })
                    .ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(Logging.Events.Containers, ex, "Failed to pause '{containerId}'", containerId);
            }
        }

        public async Task RestartContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            try
            {
                await _client
                    .For<ContainerModel>(ContainersCollection)
                    //.QueryOptions($"environment={environment}")
                    .Key(containerId)
                    .Action("Restart")
                    .Set(new { environment })
                    .ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(Logging.Events.Containers, ex, "Failed to restart '{containerId}'", containerId);
            }
        }

        public async Task StartContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            try
            {
                await _client
                    .For<ContainerModel>(ContainersCollection)
                    //.QueryOptions($"environment={environment}")
                    .Key(containerId)
                    .Action("Start")
                    .Set(new { environment })
                    .ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(Logging.Events.Containers, ex, "Failed to start '{containerId}'", containerId);
            }
        }

        public async Task StopContainerAsync(long environment, string containerId, CancellationToken cancellationToken)
        {
            try
            {
                await _client
                    .For<ContainerModel>(ContainersCollection)
                    //.QueryOptions($"environment={environment}")
                    .Key(containerId)
                    .Action("Stop")
                    .Set(new { environment })
                    .ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(Logging.Events.Containers, ex, "Failed to stop '{containerId}'", containerId);
            }
        }
    }
}
