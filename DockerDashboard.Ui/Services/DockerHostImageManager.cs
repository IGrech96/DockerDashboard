using System.Runtime.CompilerServices;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;
using DockerDashboard.Shared.Services;
using DockerDashboard.Ui.Clients;
using DockerDashboard.Ui.Logging;
using DockerDashboard.Ui.Progress;
using Microsoft.AspNetCore.Components;
using Simple.OData.Client;

namespace DockerDashboard.Ui.Services;

public class DockerHostImageManager : IDockerHostImageManager
{
    private const string ImagesCollection = "Images";
    //private const string DetailsAction = "Details";

    private readonly long _environment;
    private readonly ODataClient _client;
    private readonly ILogger<Events.UserMarker> _logger;
    private readonly NavigationManager _navigationManager;

    public DockerHostImageManager(long environment, ODataClient client, ILogger<Events.UserMarker> logger, NavigationManager navigationManager)
    {
        _environment = environment;
        _client = client;
        _logger = logger;
        _navigationManager = navigationManager;
    }

    public async IAsyncEnumerable<ImageModel> GetImagesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        //TODO: error handling
        var client = _client
            .For<ImageModel>(ImagesCollection)
            .QueryOptions($"environment={_environment}");

        var data = client.FindEntriesAllPagesAsync(cancellationToken);

        await foreach (var model in data)
        {
            yield return model;
        }
    }

    public async Task PullImageAsync(string image, IProgress<ProgressEvent> progress, CancellationToken cancellationToken)
    {
        var progressAdapter = new HubConnectionProgress(_navigationManager, progress);
        try
        {
            await progressAdapter.StartAsync();

            await _client
                .For<ContainerModel>(ImagesCollection)
                .QueryOptions($"environment={_environment}")
                .Action("Pull")
                .Set(new { environment = _environment, image, progressTrackId = progressAdapter.TrackId })
                .ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(Logging.Events.Containers, ex, "Failed to pull '{image}'", image);
        }
        finally
        {
            await progressAdapter.DisposeAsync();
        }
    }
}