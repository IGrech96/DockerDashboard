using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace DockerDashboard.Controllers;

public class ImagesController : ODataController
{
    private readonly IDockerHostManager _hostManager;

    public ImagesController(IDockerHostManager hostManager)
    {
        _hostManager = hostManager;
    }

    public async Task<ImageModel[]> Get(long environment, CancellationToken cancellationToken)
    {
        var images = await _hostManager
            .GetImageManager(environment)
            .GetImagesAsync(cancellationToken)
            .ToArrayAsync(cancellationToken);


        return images;
    }
}