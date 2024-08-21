using System.Text.Json;
using System.Text.Json.Serialization;
using DockerDashboard.Hubs;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DockerDashboard.Services.DockerHost;

public class DemoDockerHost : IDockerHost
{
    private readonly IHubContext<ContainerDetailsHub> _hubContex;
    private readonly DockerEnvironment _environment;
    private readonly List<ContainerDetailedModel> _containers;

    public DemoDockerHost(IHubContext<ContainerDetailsHub> hubContex, DockerEnvironment environment)
    {
        _hubContex = hubContex;
        _environment = environment;
        _containers = GenerateContainers().ToList();
    }

    public Task StartWatchingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<ContainerLog> GetLogsAsync(string containerId, DateTimeOffset? since, DateTimeOffset? until, long? top, CancellationToken cancellationToken)
    {
        return AsyncEnumerable.Empty<ContainerLog>();
    }

    public IAsyncEnumerable<ContainerModel> GetContainers(string? beforeContainerId, long? take, CancellationToken cancellationToken)
    {
        IEnumerable<ContainerDetailedModel> data = _containers;
        if (beforeContainerId != null)
        {
            data = data.SkipWhile(t => t.ContainerId != beforeContainerId);
        }

        if (take != null)
        {
            data = data.Take((int)take.Value);
        }

        return data.ToAsyncEnumerable();
    }

    public Task<ContainerModel?> GetContainer(string containerId, CancellationToken cancellationToken)
    {
        return Task.FromResult<ContainerModel?>(_containers.FirstOrDefault(c => c.ContainerId == containerId));
    }

    public Task<ContainerDetailedModel?> GetContainerDetails(string containerId, CancellationToken cancellationToken)
    {
        return Task.FromResult<ContainerDetailedModel?>(_containers.FirstOrDefault(c => c.ContainerId == containerId));
    }

    public async Task PauseContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        if (await GetContainer(containerId, cancellationToken) is { } container)
        {
            container.Status = ContainerStatus.Paused;
           await _hubContex.Clients.All.SendAsync(HubRouting.ContainerUpdateMethod(_environment.Id), new UpdateContainerEvent(containerId, container), cancellationToken);
        }
    }

    public async Task StopContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        if (await GetContainer(containerId, cancellationToken) is { } container)
        {
            container.Status = ContainerStatus.Paused;
            await _hubContex.Clients.All.SendAsync(HubRouting.ContainerUpdateMethod(_environment.Id), new UpdateContainerEvent(containerId, container), cancellationToken);
        }
    }

    public async Task StartContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        if (await GetContainer(containerId, cancellationToken) is { } container)
        {
            container.Status = ContainerStatus.Running;
            await _hubContex.Clients.All.SendAsync(HubRouting.ContainerUpdateMethod(_environment.Id), new UpdateContainerEvent(containerId, container), cancellationToken);
        }
    }

    public async Task DeleteContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        if (await GetContainer(containerId, cancellationToken) is { } container)
        {
            _containers.RemoveAll(c => c.ContainerId == containerId);
            await _hubContex.Clients.All.SendAsync(HubRouting.ContainerDestroyMethod(_environment.Id), new DestroyContainerEvent(containerId), cancellationToken);
        }
    }

    public async Task RestartContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        if (await GetContainer(containerId, cancellationToken) is { } container)
        {
            container.Status = ContainerStatus.Restarted;
            await _hubContex.Clients.All.SendAsync(HubRouting.ContainerUpdateMethod(_environment.Id), new UpdateContainerEvent(containerId, container), cancellationToken);
        }
    }

    public Task StopWatchingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private ContainerDetailedModel[] GenerateContainers()
    {
        var options = new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter() }
        };
        return JsonSerializer.Deserialize<DemoContainers>(Demo.demo_containers, options)!.value;
    }

    public class DemoContainers
    {
        public ContainerDetailedModel[] value { get; set; } = [];
    }
}