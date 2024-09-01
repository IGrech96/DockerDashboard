using System.Runtime.CompilerServices;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services;
using DockerDashboard.Ui.Clients;
using DockerDashboard.Ui.Logging;
using Simple.OData.Client;

namespace DockerDashboard.Ui.Services;

public class DockerHostImageManager : IDockerHostImageManager
{
    private const string ImagesCollection = "Images";
    //private const string DetailsAction = "Details";

    private readonly long _environment;
    private readonly ODataClient _client;
    private readonly ILogger<Events.UserMarker> _logger;

    public DockerHostImageManager(long environment, ODataClient client, ILogger<Events.UserMarker> logger)
    {
        _environment = environment;
        _client = client;
        _logger = logger;
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
}