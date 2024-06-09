using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DockerDashboard.Data;

public class DockerHostManager : IDockerHostManager
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDockerClient client_;
    public DockerHostManager(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        client_ = new DockerClientConfiguration().CreateClient();
    }

    private ContainerStatus MapStatus(string status) => status switch
    {
        {} d when d.StartsWith("Exited") => ContainerStatus.Exited,
        _ => ContainerStatus.NA
    };

    public async Task<ContainersSnapshot> GetContainersSnapshot()
    {
        var response = await client_
            .Containers
            .ListContainersAsync(new() { All = true });

        var id = Guid.NewGuid();

        _memoryCache.Set(id, response);

        return new ContainersSnapshot()
        {
            SnapshotId = id,
            TotalCount = response.Count
        };
    }

    public IAsyncEnumerable<ContainerModel> GetContainers(Guid snapshotId, int startIndex, int count)
    {
        var data = _memoryCache.Get<IList<ContainerListResponse>>(snapshotId) ?? [];

        return data.Skip(startIndex).Take(count).Select(ToContainer).ToAsyncEnumerable();
    }

    private ContainerModel ToContainer(ContainerListResponse response)
    {
        return new ContainerModel()
        {
            ContainerId = response.ID,
            ContainerName = response.Names.First(), // todo,
            Status = MapStatus(response.Status),
            Created = response.Created,
            ImageName = response.Image,
            Ports = response.Ports.Select(p => (p.PublicPort, p.PublicPort)).ToArray(), //todo
        };
    }
}