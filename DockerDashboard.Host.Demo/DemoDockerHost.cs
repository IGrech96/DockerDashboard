﻿using System.Text.Json;
using System.Text.Json.Serialization;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;
using DockerDashboard.Shared.Messaging;
using DockerDashboard.Shared.Services;

namespace DockerDashboard.Host.Demo;

internal class DemoDockerHost : IDockerHost, IDockerHostContainerManager, IDockerHostImageManager
{
    private readonly IMessageBus _hubContex;
    private readonly DockerEnvironment _environment;
    private readonly List<ContainerDetailedModel> _containers;
    private readonly List<ImageModel> _images;

    public IDockerHostContainerManager ContainersHost => this;
    public IDockerHostImageManager ImagesHost => this;

    public DemoDockerHost(IMessageBus hubContex, DockerEnvironment environment)
    {
        _hubContex = hubContex;
        _environment = environment;
        _containers = Load<DemoContainers, ContainerDetailedModel>(Demo.demo_containers);
        _images = Load<DemoImages, ImageModel>(Demo.demo_images);
    }

    public Task StartWatchingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<ContainerLog> GetContainerLogsAsync(string containerId, DateTimeOffset? since, DateTimeOffset? until, long? top, CancellationToken cancellationToken)
    {
        return AsyncEnumerable.Empty<ContainerLog>();
    }

    public IAsyncEnumerable<ContainerModel> GetContainersAsync(string? beforeContainerId, long? take, CancellationToken cancellationToken)
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

    public Task<ContainerModel?> TryGetContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        return Task.FromResult<ContainerModel?>(_containers.FirstOrDefault(c => c.ContainerId == containerId));
    }

    public Task<ContainerDetailedModel?> TryGetContainerDetailsAsync(string containerId, CancellationToken cancellationToken)
    {
        return Task.FromResult<ContainerDetailedModel?>(_containers.FirstOrDefault(c => c.ContainerId == containerId));
    }

    public async Task PauseContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        if (await TryGetContainerAsync(containerId, cancellationToken) is { } container)
        {
            container.Status = ContainerStatus.Paused;
           await _hubContex.SendToAllAsync(HubRouting.ContainerUpdateMethod(_environment.Id), new UpdateContainerEvent(containerId, container), cancellationToken);
        }
    }

    public async Task StopContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        if (await TryGetContainerAsync(containerId, cancellationToken) is { } container)
        {
            container.Status = ContainerStatus.Paused;
            await _hubContex.SendToAllAsync(HubRouting.ContainerUpdateMethod(_environment.Id), new UpdateContainerEvent(containerId, container), cancellationToken);
        }
    }

    public async Task StartContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        if (await TryGetContainerAsync(containerId, cancellationToken) is { } container)
        {
            container.Status = ContainerStatus.Running;
            await _hubContex.SendToAllAsync(HubRouting.ContainerUpdateMethod(_environment.Id), new UpdateContainerEvent(containerId, container), cancellationToken);
        }
    }

    public async Task DeleteContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        if (await TryGetContainerAsync(containerId, cancellationToken) is { } container)
        {
            _containers.RemoveAll(c => c.ContainerId == containerId);
            await _hubContex.SendToAllAsync(HubRouting.ContainerDestroyMethod(_environment.Id), new DestroyContainerEvent(containerId), cancellationToken);
        }
    }

    public async Task RestartContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        if (await TryGetContainerAsync(containerId, cancellationToken) is { } container)
        {
            container.Status = ContainerStatus.Restarted;
            await _hubContex.SendToAllAsync(HubRouting.ContainerUpdateMethod(_environment.Id), new UpdateContainerEvent(containerId, container), cancellationToken);
        }
    }

    public Task StopWatchingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    

    public async Task RecreateContainerAsync(string containerId, bool pullImage, IProgress<ProgressEvent> progress, CancellationToken cancellationToken)
    {
        if (await TryGetContainerAsync(containerId, cancellationToken) is { } container)
        {
            await _hubContex.SendToAllAsync(HubRouting.ContainerDestroyMethod(_environment.Id), new DestroyContainerEvent(containerId), cancellationToken);

            container.Status = ContainerStatus.Running;

            await _hubContex.SendToAllAsync(HubRouting.ContainerCreateMethod(_environment.Id), new CreateContainerEvent(containerId, container), cancellationToken);
        }
    }

    public IAsyncEnumerable<ImageModel> GetImagesAsync(CancellationToken cancellationToken)
    {
        return _images.ToAsyncEnumerable();
    }

    public Task PullImageAsync(string image, IProgress<ProgressEvent> progress, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<ImageModel?> TryGetImageAsync(string imageId, CancellationToken cancellationToken)
    {
        var data = _images.First(i => i.ImageId == imageId);
        return Task.FromResult<ImageModel?>(data);
    }

    public Task DeleteImageAsync(string imageId, CancellationToken cancellationToken)
    {
        _images.RemoveAll(r => r.ImageId == imageId);
        return Task.CompletedTask;
    }

    private List<TOutput> Load<TWrapper,TOutput>(byte[] data) where TWrapper : IDemoWrapper<TOutput>
    {
        var options = new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter() }
        };
        return JsonSerializer.Deserialize<TWrapper>(data, options)!.value.ToList();
    }

    public class DemoContainers : IDemoWrapper<ContainerDetailedModel>
    {
        public ContainerDetailedModel[] value { get; set; } = [];
    }

    public class DemoImages : IDemoWrapper<ImageModel>
    {
        public ImageModel[] value { get; set; } = [];
    }

    public interface IDemoWrapper<out T>
    {
        T[] value { get; }
    }
}