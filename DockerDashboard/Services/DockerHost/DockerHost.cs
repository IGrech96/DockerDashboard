using Docker.DotNet;
using Docker.DotNet.Models;
using DockerDashboard.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json.Serialization;
using DockerDashboard.Services.Environment;
using System.Runtime.CompilerServices;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;

namespace DockerDashboard.Services.DockerHost;

public class DockerHost
{
    private readonly IDockerClient _client;
    private readonly IHubContext<ContainerDetailsHub> _containerDetailsHub;
    private readonly DockerEnvironment _environment;
    private CancellationTokenSource? _watchContainerEventsTokenSource;

    public DockerHost(IHubContext<ContainerDetailsHub> containerDetailsHub, DockerEnvironment environment)
    {
        _client = new DockerClientConfiguration().CreateClient();
        _containerDetailsHub = containerDetailsHub;
        _environment = environment;
    }

    internal Task StartWatchingAsync(CancellationToken cancellationToken)
    {
        _watchContainerEventsTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        MonitorEventsAsync(_watchContainerEventsTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        return Task.CompletedTask;
    }

    internal async IAsyncEnumerable<string> GetLogsAsync(string containerId, DateTimeOffset since, DateTimeOffset until, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var multiStream = await _client.Containers.GetContainerLogsAsync(containerId, false, new ContainerLogsParameters
        {
            ShowStderr = true,
            ShowStdout = true,
            Timestamps = true,
            Since = since.ToUnixTimeSeconds().ToString(),
            Until = until.ToUnixTimeSeconds().ToString()
        },
        cancellationToken);

        var underlyingStream = multiStream.GetStream();
        using var reader = new StreamReader(underlyingStream, Encoding.UTF8, false);
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;
            yield return line;
        }
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

    public async IAsyncEnumerable<ContainerModel> GetContainers(string? beforeContainerId, long? take, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var parameters = new ContainersListParameters()
        {
            Limit = take,
        };
        if (beforeContainerId != null)
        {
            parameters.Filters = new Dictionary<string, IDictionary<string, bool>>()
            {
                {
                    "before",
                    new Dictionary<string, bool>() { { beforeContainerId, true } }
                }
            };
        }

        var data = await _client
            .Containers
            .ListContainersAsync(parameters, cancellationToken);

        foreach (var container in data) 
        {
            yield return ToContainer(container);
        }
    }

    
    public async Task<ContainerModel> GetContainer(string containerId, CancellationToken cancellationToken)
    {
        var data = await _client.Containers.ListContainersAsync(new ContainersListParameters()
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>(){ {"id", new Dictionary<string, bool>() { {containerId, true } } } },
            All=true,
        }, cancellationToken);

        return ToContainer(data.First());
    }

    public async Task<ContainerDetailedModel> GetContainerDetails(string containerId, CancellationToken cancellationToken)
    {
        var data = await _client.Containers.InspectContainerAsync(containerId, cancellationToken);

        var result = ToContainer(data);
        return result;
    }

    public async Task PauseContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        await _client.Containers.PauseContainerAsync(containerId, cancellationToken);
    }

    public async Task StopContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        await _client.Containers.StopContainerAsync(containerId, new(), cancellationToken);
    }

    public async Task StartContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        await _client.Containers.StartContainerAsync(containerId, new(), cancellationToken);
    }

    public async Task DeleteContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        await _client.Containers.RemoveContainerAsync(containerId, new(), cancellationToken);
    }

    public async Task RestartContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        await _client.Containers.RestartContainerAsync(containerId, new(), cancellationToken);
    }


    internal async Task StopWatchingAsync(CancellationToken cancellationToken)
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
            "create" => new CreateContainerEvent(message.ID, await GetContainer(message.ID, cancellationToken)),
            "destroy" => new DestroyContainerEvent(message.ID),
            _ when !string.IsNullOrWhiteSpace(message.ID) => new UpdateContainerEvent(message.ID, await GetContainer(message.ID, cancellationToken)),
            _ => null,
        };
        
        if (@event is not null) 
        {
           await _containerDetailsHub.Clients.All.SendAsync(HubRouting.ContainerUpdateMethod(_environment.Id), @event, cancellationToken);
        }

    }

    private Shared.Data.ContainerStatus MapStatus(string status) => status.ToLower() switch
    {
        { } d when d.StartsWith("exited") => Shared.Data.ContainerStatus.Exited,
        { } d when d.StartsWith("created") => Shared.Data.ContainerStatus.Created,
        { } d when d.StartsWith("restarting") => Shared.Data.ContainerStatus.Restarted,
        { } d when d.StartsWith("running") => Shared.Data.ContainerStatus.Running,
        { } d when d.StartsWith("removing") => Shared.Data.ContainerStatus.Removing,
        { } d when d.StartsWith("paused") => Shared.Data.ContainerStatus.Paused,
        { } d when d.StartsWith("dead") => Shared.Data.ContainerStatus.Dead,
        _ => Shared.Data.ContainerStatus.NA
    };

    private ContainerDetailedModel ToContainer(ContainerInspectResponse response)
    {
        return new ContainerDetailedModel()
        {
            ContainerId = response.ID,
            ContainerName = response.Name,
            Status = MapStatus(response.State.Status),
            Created = response.Created,
            ImageName = response.Image,
            EntryPoint = response.Config.Entrypoint?.ToArray() ?? [],
            WorkingDirectory = response.Config.WorkingDir,
            Command = response.Config.Cmd.ToArray(),
            User = response.Config.User,
            Environment = ConvertEnvironment().Select(v => new KeyValue(v.Key, v.Value)).ToArray(),
            Labels = response.Config.Labels.Select(v => new KeyValue(v.Key, v.Value)).ToArray(),
            RestartCount = response.RestartCount,
            RestartPolicy = MapRestartPolicy(response.HostConfig.RestartPolicy.Name),
            Mounts = response.Mounts.Select(MapMountPoint).ToArray(),
            //Networks = response.NetworkSettings.
            //Ports = response.Config.Ports.Select(p => (p.PublicPort, p.PublicPort)).ToArray(), //todo
        };

        Dictionary<string, string> ConvertEnvironment()
        {
            return response
                .Config
                .Env
                .Where(d => d.Contains("="))
                .Select(d => d.Split("=") is {Length: > 1} array ? (array[0], string.Join("=", array.Skip(1))) : ("", ""))
                .DistinctBy(_ => _.Item1)
                .ToDictionary(_ => _.Item1, _ => _.Item2);
        }
    }

    private Shared.Data.MountPoint MapMountPoint(Docker.DotNet.Models.MountPoint point)
    {
        return new Shared.Data.MountPoint
        {
            Destination = point.Destination,
            Driver = point.Driver,
            Mode = point.Mode,
            Name = point.Name,
            Propagation = point.Propagation,
            RW = point.RW,
            Source = point.Source,
            Type = point.Type,
        };
    }

    private Shared.Data.RestartPolicy MapRestartPolicy(RestartPolicyKind name)
    {
        return name switch
        {
            RestartPolicyKind.Undefined => Shared.Data.RestartPolicy.Undefined,
            RestartPolicyKind.No => Shared.Data.RestartPolicy.No,
            RestartPolicyKind.Always => Shared.Data.RestartPolicy.Always,
            RestartPolicyKind.OnFailure => Shared.Data.RestartPolicy.OnFailure,
            RestartPolicyKind.UnlessStopped => Shared.Data.RestartPolicy.UnlessStopped,
            _ => Shared.Data.RestartPolicy.Undefined,
        };
    }

    private ContainerModel ToContainer(ContainerListResponse response)
    {
        return new ContainerModel()
        {
            ContainerId = response.ID,
            ContainerName = response.Names.First(), // todo,
            Status = MapStatus(response.State),
            Created = response.Created,
            ImageName = response.Image,
            Ports = response.Ports.Select(p => new ContainerPort(p.PublicPort, p.PrivatePort)).ToArray(), //todo
        };
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