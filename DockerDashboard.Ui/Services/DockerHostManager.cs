using Simple.OData.Client;
using DockerDashboard.Shared.Services;
using DockerDashboard.Ui.Clients;
using static DockerDashboard.Ui.Logging.Events;

namespace DockerDashboard.Ui.Services;

public class DockerHostManager : IDockerHostManager
{
    private readonly ODataClient _client;
    private readonly ILogger<UserMarker> _logger;

    public IDockerHostContainerManager GetContainerManager(long environment)
    {
        return new DockerHostContainerManager(environment, _client, _logger);
    }

    public IDockerHostImageManager GetImageManager(long environment)
    {
        return new DockerHostImageManager(environment, _client, _logger);
    }

    public DockerHostManager(
        [FromKeyedServices(ClientCategory.Backend)]
        ODataClient client,
        ILogger<UserMarker> logger)
    {
        _client = client;
        _logger = logger;
    }

}