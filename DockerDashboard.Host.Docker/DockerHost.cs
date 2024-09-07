using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;
using DockerDashboard.Shared.Messaging;
using DockerDashboard.Shared.Services;

namespace DockerDashboard.Host.Docker;

internal class DockerHost : IDockerHost
{
    private readonly IDockerClient _client;
    private readonly IMessageBus _containerDetailsHub;
    private readonly DockerEnvironment _environment;
    private CancellationTokenSource? _watchContainerEventsTokenSource;

    public IDockerHostContainerManager ContainersHost { get; }
    public IDockerHostImageManager ImagesHost { get; }

    public DockerHost(
        IMessageBus containerDetailsHub,
        IDockerRegistryManager registryManager,
        DockerEnvironment environment)
    {
        _client = new DockerClientConfiguration().CreateClient();
        _containerDetailsHub = containerDetailsHub;
        _environment = environment;

        ImagesHost = new DockerImagesHost(_client, containerDetailsHub, registryManager, environment);
        ContainersHost = new DockerContainersHost(_client, _containerDetailsHub, ImagesHost, _environment);
    }

    public Task StartWatchingAsync(CancellationToken cancellationToken)
    {
        _watchContainerEventsTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        MonitorEventsAsync(_watchContainerEventsTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        return Task.CompletedTask;
    }
    
    private async Task MonitorEventsAsync(CancellationToken cancellationToken)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        await using var stream = await _client.System.MonitorEventsAsync(new ContainerEventsParameters(), cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
        using var reader = new StreamReader(stream, Encoding.UTF8, false);
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;
            var msg = System.Text.Json.JsonSerializer.Deserialize<DockerMessage>(line);
            if (msg is null) continue;
            await OnDockerEventAsync(msg, cancellationToken);
        }
    }

    

    public async Task StopWatchingAsync(CancellationToken cancellationToken)
    {
        if (_watchContainerEventsTokenSource != null)
        {
            await _watchContainerEventsTokenSource.CancelAsync();
        }

        _client.Dispose();
    }

    private async Task OnDockerEventAsync(DockerMessage message, CancellationToken cancellationToken)
    {
        ContainerEvent? @event = message.Status switch
        {
            "create" => new CreateContainerEvent(message.ID, (await ContainersHost.TryGetContainerAsync(message.ID, cancellationToken))!),
            "destroy" => new DestroyContainerEvent(message.ID),
            _ when !string.IsNullOrWhiteSpace(message.ID) => new UpdateContainerEvent(message.ID, (await ContainersHost.TryGetContainerAsync(message.ID, cancellationToken))!),
            _ => null,
        };


        var methodName = @event switch
        {
            CreateContainerEvent create => HubRouting.ContainerCreateMethod(_environment.Id),
            DestroyContainerEvent destroy => HubRouting.ContainerDestroyMethod(_environment.Id),
            _ => HubRouting.ContainerUpdateMethod(_environment.Id)
        };
        if (@event is not null) 
        {
           await _containerDetailsHub.SendToAllAsync(methodName, @event, cancellationToken);
        }

    }

    public class DockerMessage
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("id")]
        public required string ID { get; set; }

        [JsonPropertyName("from")]
        public required string From { get; set; }

        [JsonPropertyName("Type")]
        public required string Type { get; set; }

        [JsonPropertyName("Action")]
        public required string Action { get; set; }

        [JsonPropertyName("Actor")]
        public DockerActor? Actor { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("time")]
        public required long Time { get; set; }

        [JsonPropertyName("timeNano")]
        public required long TimeNano { get; set; }
    }

    public class DockerActor // (events.Actor)
    {
        [JsonPropertyName("ID")]
        public required string ID { get; set; }

        [JsonPropertyName("Attributes")]
        public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }

}

public static class MultiplexedStreamExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_stream")]
    static extern ref Stream stream_(MultiplexedStream stream); 

    public static Stream GetStream(this MultiplexedStream stream) => stream_(stream);
}