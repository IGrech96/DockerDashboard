using DockerDashboard.Progress;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.SignalR;
using System;
using DockerDashboard.Hubs;
using DockerDashboard.Shared.Hubs;

namespace DockerDashboard.Controllers;

public class ImagesController : ODataController
{
    private readonly IDockerHostManager _hostManager;
    private readonly IHubContext<ContainerDetailsHub> _hub;

    public ImagesController(IDockerHostManager hostManager, IHubContext<ContainerDetailsHub> hub)
    {
        _hostManager = hostManager;
        _hub = hub;
    }

    public async Task<ImageModel[]> Get(long environment, CancellationToken cancellationToken)
    {
        var images = await _hostManager
            .GetImageManager(environment)
            .GetImagesAsync(cancellationToken)
            .ToArrayAsync(cancellationToken);


        return images;
    }

    public async Task<ImageModel?> Get(long environment, [FromRoute]  string key, CancellationToken cancellationToken)
    {
        var image = await _hostManager
            .GetImageManager(environment)
            .TryGetImageAsync(key, cancellationToken);

        return image;
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(long environment,[FromRoute]  string key, CancellationToken cancellationToken)
    {
        await _hostManager
            .GetImageManager(environment)
            .DeleteImageAsync(key, cancellationToken);


        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Pull(ODataActionParameters? parameters, CancellationToken cancellationToken)
    {
        if (parameters?.TryGetValue("environment", out var arg) != true || arg is not long {} environment)
        {
            return BadRequest("'environment' is not provided.");
        }

        if (parameters?.TryGetValue("image", out var arg2) != true || arg2 is not string {} image)
        {
            return BadRequest("'image' is not provided.");
        }

        IProgress<ProgressEvent> progress = new Progress<ProgressEvent>();
        if (parameters?.TryGetValue("progressTrackId", out var arg3) == true && arg3 is string { } progressTrackId)
        {
            progress = new HubProgress(progressTrackId, _hub);
        }
        await _hostManager.GetImageManager(environment).PullImageAsync(image, progress, cancellationToken);
        return NoContent();
    }
}