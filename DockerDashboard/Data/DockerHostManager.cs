using Docker.DotNet;
using Docker.DotNet.Models;
using DockerDashboard.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace DockerDashboard.Data;

public class DockerHostManager : IDockerHostManager
{
    private readonly IMemoryCache _memoryCache;
    private readonly IHubContext<ContainerDetailsHub> _containerDetailsHub;
    private readonly IDockerClient client_;
    public DockerHostManager(
        IMemoryCache memoryCache,
        IHubContext<ContainerDetailsHub> containerDetailsHub)
    {
        _memoryCache = memoryCache;
        _containerDetailsHub = containerDetailsHub;
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

    public Task SubscribeToContainerDetails(string containerId)
    {
        client_.System.MonitorEventsAsync(new(), new Progress<Message>(m =>
        {

        }));

        return Task.CompletedTask;
    }

    public IAsyncEnumerable<ContainerModel> GetContainers(Guid snapshotId)
    {
        var data = _memoryCache.Get<IList<ContainerListResponse>>(snapshotId) ?? [];
        return data.Select(ToContainer).ToAsyncEnumerable();
    }

    private ContainerModel ToContainer(ContainerListResponse response)
    {
        return new ContainerModel()
        {
            ContainerId = response.ID,
            ShortId = response.ID.Substring(0, 12),
            ContainerName = response.Names.First(), // todo,
            Status = MapStatus(response.Status),
            Created = response.Created,
            ImageName = response.Image,
            Ports = response.Ports.Select(p => (p.PublicPort, p.PublicPort)).ToArray(), //todo
        };
    }

    //private ContainerModel ToContainerDetails(ContainerStatsResponse response)
    //{
    //    return new ContainerModel()
    //    {
    //        ContainerId = response.ID,
    //        ShortId = response.ID.Substring(0, 12),
    //        ContainerName = response.Names.First(), // todo,
    //        Status = MapStatus(response.),
    //        Created = response.Created,
    //        ImageName = response.Image,
    //        Ports = response.Ports.Select(p => (p.PublicPort, p.PublicPort)).ToArray(), //todo
    //    };
    //}
}