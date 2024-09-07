using System.Runtime.CompilerServices;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;
using DockerDashboard.Shared.Services;
using DockerDashboard.Ui.Clients;
using Simple.OData.Client;

namespace DockerDashboard.Ui.Services;

public class DockerHostContainerManager : IDockerHostContainerManager
{
    private const string ContainersCollection = "Containers";
    private const string DetailsAction = "Details";

    private readonly long _environment;
    private readonly ODataClient _client;
    private readonly ILogger _logger;

    public DockerHostContainerManager(long environment, ODataClient client, ILogger logger)
    {
        _environment = environment;
        _client = client;
        _logger = logger;
    }
    public async Task<ContainerModel?> TryGetContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _client
                .For<ContainerModel>(ContainersCollection)
                .QueryOptions($"environment={_environment}")
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

    public async Task<ContainerDetailedModel?> TryGetContainerDetailsAsync(string containerId, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _client
                .For<ContainerModel>(ContainersCollection)
                //.QueryOptions($"environment={environment}")
                .Key(containerId)
                .Function<ContainerDetailedModel>(DetailsAction)
                .Set(new { _environment })
                .ExecuteAsSingleAsync(cancellationToken);

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(Logging.Events.Containers, ex, "Failed to find container details '{containerId}'", containerId);
            return null;
        }
    }

    public async IAsyncEnumerable<ContainerLog> GetContainerLogsAsync(string containerId, DateTimeOffset? since, DateTimeOffset? until, long? top, [EnumeratorCancellation] CancellationToken cancellationToken)
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
            .Set(new { _environment, until, since })
            .ExecuteAsArrayAsync(cancellationToken);

        foreach (var item in data)
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<ContainerModel> GetContainersAsync(string? beforeContainerId, long? take, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        //TODO: error handling
        var client = _client
            .For<ContainerModel>(ContainersCollection)
            .QueryOptions($"environment={_environment}");

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

    public async Task DeleteContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        try
        {
            await _client
                .For<ContainerModel>(ContainersCollection)
                .QueryOptions($"environment={_environment}")
                .Key(containerId)
                .DeleteEntryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(Logging.Events.Containers, ex, "Failed to delete '{containerId}'", containerId);
        }
    }

    public async Task PauseContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        try
        {
            await _client
                .For<ContainerModel>(ContainersCollection)
                //.QueryOptions($"environment={environment}")
                .Key(containerId)
                .Action("Pause")
                .Set(new { _environment })
                .ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(Logging.Events.Containers, ex, "Failed to pause '{containerId}'", containerId);
        }
    }

    public async Task RestartContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        try
        {
            await _client
                .For<ContainerModel>(ContainersCollection)
                //.QueryOptions($"environment={environment}")
                .Key(containerId)
                .Action("Restart")
                .Set(new { _environment })
                .ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(Logging.Events.Containers, ex, "Failed to restart '{containerId}'", containerId);
        }
    }

    public async Task RecreateContainerAsync(string containerId, bool pullImage, IProgress<ProgressEvent> progress, CancellationToken cancellationToken)
    {
        try
        {
            await _client
                .For<ContainerModel>(ContainersCollection)
                //.QueryOptions($"environment={environment}")
                .Key(containerId)
                .Action("Recreate")
                .Set(new { _environment, pullImage })
                .ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(Logging.Events.Containers, ex, "Failed to recreate '{containerId}'", containerId);
        }
    }

    public async Task StartContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        try
        {
            await _client
                .For<ContainerModel>(ContainersCollection)
                //.QueryOptions($"environment={environment}")
                .Key(containerId)
                .Action("Start")
                .Set(new { _environment })
                .ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(Logging.Events.Containers, ex, "Failed to start '{containerId}'", containerId);
        }
    }

    public async Task StopContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        try
        {
            await _client
                .For<ContainerModel>(ContainersCollection)
                //.QueryOptions($"environment={environment}")
                .Key(containerId)
                .Action("Stop")
                .Set(new { _environment })
                .ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(Logging.Events.Containers, ex, "Failed to stop '{containerId}'", containerId);
        }
    }
}