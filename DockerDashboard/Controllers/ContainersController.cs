using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace DockerDashboard.Controllers;

public class ContainersController : ODataController
{
    private readonly IDockerHostManager _hostManager;

    public ContainersController(IDockerHostManager hostManager)
    {
        _hostManager = hostManager;
    }

    public async Task<PageResult<ContainerModel>> Get(ODataQueryOptions<ContainerModel> options, long environment, CancellationToken cancellationToken)
    {
        var top = options.Top?.Value ?? 3;
        var before = options.SkipToken?.RawValue;

        var containers = await _hostManager.GetContainers(environment, before, top, cancellationToken).ToArrayAsync(cancellationToken);

        return new PageResult<ContainerModel>(
            containers,
            containers.Length >= top ? Request.GetNextPageLink(top, containers.Last(), _ => containers.Last().ContainerId ) : null,
            null);
    }

    public async Task<ContainerModel?> Get(long environment, [FromRoute] string key,  CancellationToken cancellationToken)
    {
        var data = await _hostManager.TryGetContainerAsync(environment, key, cancellationToken);
        return data;
    }

    [HttpGet]
    public async Task<ContainerDetailedModel?> Details(long environment, [FromODataUri] string key, CancellationToken cancellationToken)
    {
        var data = await _hostManager.TryGetContainerDetails(environment, key, cancellationToken);
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
        var data = await _hostManager
            .GetContainerLogsAsync(environment, key, since, until, top, cancellationToken)
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
        await _hostManager.StopContainerAsync(environment, key, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Start([FromRoute] string key, ODataActionParameters? parameters, CancellationToken cancellationToken)
    {
        if (parameters?.TryGetValue("environment", out var arg) != true || arg is not long {} environment)
        {
            return BadRequest("'environment' is not provided.");
        }
        await _hostManager.StartContainerAsync(environment, key, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(long environment, [FromRoute] string key, CancellationToken cancellationToken)
    {
        await _hostManager.DeleteContainerAsync(environment, key, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Restart([FromRoute] string key, ODataActionParameters? parameters, CancellationToken cancellationToken)
    {
        if (parameters?.TryGetValue("environment", out var arg) != true || arg is not long {} environment)
        {
            return BadRequest("'environment' is not provided.");
        }
        await _hostManager.RestartContainerAsync(environment, key, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Pause([FromRoute] string key, ODataActionParameters? parameters, CancellationToken cancellationToken)
    {
        if (parameters?.TryGetValue("environment", out var arg) != true || arg is not long {} environment)
        {
            return BadRequest("'environment' is not provided.");
        }
        await _hostManager.PauseContainerAsync(environment, key, cancellationToken);
        return NoContent();
    }

}