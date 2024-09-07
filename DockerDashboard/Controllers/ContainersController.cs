using DockerDashboard.Hubs;
using DockerDashboard.Progress;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;
using DockerDashboard.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.SignalR;

namespace DockerDashboard.Controllers;

public class ContainersController : ODataController
{
    private readonly IDockerHostManager _hostManager;
    private readonly IHubContext<ContainerDetailsHub> _hub;

    public ContainersController(IDockerHostManager hostManager, IHubContext<ContainerDetailsHub> hub)
    {
        _hostManager = hostManager;
        _hub = hub;
    }

    public async Task<PageResult<ContainerModel>> Get(ODataQueryOptions<ContainerModel> options, long environment, CancellationToken cancellationToken)
    {
        var top = options.Top?.Value ?? 3;
        var before = options.SkipToken?.RawValue;

        var containers = await _hostManager.GetContainerManager(environment).GetContainersAsync(before, top, cancellationToken).ToArrayAsync(cancellationToken);

        return new PageResult<ContainerModel>(
            containers,
            containers.Length >= top ? Request.GetNextPageLink(top, containers.Last(), _ => containers.Last().ContainerId ) : null,
            null);
    }

    public async Task<ContainerModel?> Get(long environment, [FromRoute] string key,  CancellationToken cancellationToken)
    {
        var data = await _hostManager.GetContainerManager(environment).TryGetContainerAsync(key, cancellationToken);
        return data;
    }

    [HttpGet]
    public async Task<ContainerDetailedModel?> Details(long environment, [FromODataUri] string key, CancellationToken cancellationToken)
    {
        var data = await _hostManager.GetContainerManager(environment).TryGetContainerDetailsAsync(key, cancellationToken);
        return data;
    }

    [HttpGet]
    public async  Task<ContainerLog[]> Logs(
        ODataQueryOptions<ContainerModel> options,
        long environment, 
        [FromODataUri] string key,
        DateTimeOffset? until,
        DateTimeOffset? since, 
        CancellationToken cancellationToken)
    {
        var top = (long?)options.Top?.Value;
        var data = await _hostManager.GetContainerManager(environment)
            .GetContainerLogsAsync(key, since, until, top, cancellationToken)
            .ToArrayAsync(cancellationToken: cancellationToken);

        return data;
    }

    [HttpPost]
    public async Task<ActionResult> Stop([FromRoute] string key, ODataActionParameters? parameters, CancellationToken cancellationToken)
    {
        if (parameters?.TryGetValue("environment", out var arg) != true || arg is not long {} environment)
        {
            return BadRequest("'environment' is not provided.");
        }
        await _hostManager.GetContainerManager(environment).StopContainerAsync(key, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Start([FromRoute] string key, ODataActionParameters? parameters, CancellationToken cancellationToken)
    {
        if (parameters?.TryGetValue("environment", out var arg) != true || arg is not long {} environment)
        {
            return BadRequest("'environment' is not provided.");
        }
        await _hostManager.GetContainerManager(environment).StartContainerAsync(key, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(long environment, [FromRoute] string key, CancellationToken cancellationToken)
    {
        await _hostManager.GetContainerManager(environment).DeleteContainerAsync(key, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Restart([FromRoute] string key, ODataActionParameters? parameters, CancellationToken cancellationToken)
    {
        if (parameters?.TryGetValue("environment", out var arg) != true || arg is not long {} environment)
        {
            return BadRequest("'environment' is not provided.");
        }
        await _hostManager.GetContainerManager(environment).RestartContainerAsync(key, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Pause([FromRoute] string key, ODataActionParameters? parameters, CancellationToken cancellationToken)
    {
        if (parameters?.TryGetValue("environment", out var arg) != true || arg is not long {} environment)
        {
            return BadRequest("'environment' is not provided.");
        }
        await _hostManager.GetContainerManager(environment).PauseContainerAsync(key, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Recreate([FromRoute] string key, ODataActionParameters? parameters, CancellationToken cancellationToken)
    {
        if (parameters?.TryGetValue("environment", out var arg) != true || arg is not long {} environment)
        {
            return BadRequest("'environment' is not provided.");
        }

        bool pullImage = false;

        if (parameters?.TryGetValue("pullImage", out var arg2) == true)
        {
            if (arg2 is not bool maybePull)
            {
                return BadRequest("'pull image' is not provided.");
            }
            else
            {
                pullImage = maybePull;
            }
        }

        IProgress<ProgressEvent> progress = new Progress<ProgressEvent>();

        if (parameters?.TryGetValue("progressTrackId", out var arg3) == true && arg3 is string { } progressTrackId)
        {
            progress = new HubProgress(progressTrackId, _hub);
        }

        await _hostManager.GetContainerManager(environment).RecreateContainerAsync(key, pullImage, progress, cancellationToken);
        return NoContent();
    }

}