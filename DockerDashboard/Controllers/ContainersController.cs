using DockerDashboard.Services.DockerHost;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
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

    [HttpGet()]
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

    [HttpGet("odata/Containers/({containerId})")]
    public async Task<ContainerModel> GetById([FromQuery] long environment, [FromRoute] string containerId, CancellationToken cancellationToken)
    {
        return await _hostManager.GetContainerAsync(environment, containerId, cancellationToken);
    }
}