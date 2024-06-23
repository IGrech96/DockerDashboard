using Docker.DotNet;
using Docker.DotNet.Models;
using DockerDashboard.Data;
using DockerDashboard.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json.Serialization;
using DockerDashboard.Services.Environment;
using System.Threading;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.IO;

namespace DockerDashboard.Services.DockerHost;

public class DockerHost
{
    private readonly IDockerClient client_;
    private readonly IHubContext<ContainerDetailsHub> containerDetailsHub_;
    private readonly DockerEnvironment environment_;
    private CancellationTokenSource watchContainerEventsTokenSource;

    public DockerHost(IHubContext<ContainerDetailsHub> containerDetailsHub, DockerEnvironment environment)
    {
        client_ = new DockerClientConfiguration().CreateClient();
        containerDetailsHub_ = containerDetailsHub;
        environment_ = environment;
    }

    internal async Task StartWatchingAsync(CancellationToken cancellationToken)
    {
        watchContainerEventsTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        MonitorEventsAsync(watchContainerEventsTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    internal async Task<string[]> GetLogsAsync(string containerId, DateTimeOffset since, DateTimeOffset until, CancellationToken cancellationToken)
    {
        using var multiStream = await client_.Containers.GetContainerLogsAsync(containerId, false, new ContainerLogsParameters
        {
            ShowStderr = true,
            ShowStdout = true,
            Timestamps = true,
            Since = since.ToUnixTimeSeconds().ToString(),
            Until = until.ToUnixTimeSeconds().ToString()
        },
        cancellationToken);

        var results = new List<string>();
        var underlyingStream = multiStream.GetStream();
        using var reader = new StreamReader(underlyingStream, Encoding.UTF8, false);
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;
           results.Add(line);
        }

        return results.ToArray();
    }

   

    private async Task MonitorEventsAsync(CancellationToken cancellationToken)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        using var stream = await client_.System.MonitorEventsAsync(new ContainerEventsParameters(), cancellationToken);
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

    public async IAsyncEnumerable<ContainerModel> GetContainers(CancellationToken cancellationToken)
    {
        var data = await client_.Containers.ListContainersAsync(new (){All=true}, cancellationToken);

        foreach (var container in data) 
        {
            yield return ToContainer(container);
        }
    }

    
    public async Task<ContainerModel> GetContainer(string containerId, CancellationToken cancellationToken)
    {
        var data = await client_.Containers.ListContainersAsync(new ContainersListParameters()
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>(){ {"id", new Dictionary<string, bool>() { {containerId, true } } } },
            All=true,
        }, cancellationToken);

        return ToContainer(data.First());
    }

    public async Task<ContainerDetailedModel> GetContainerDetails(string containerId, CancellationToken cancellationToken)
    {
        var data = await client_.Containers.InspectContainerAsync(containerId, cancellationToken);

        var result = ToContainer(data);
        return result;
    }


    internal async Task StopWatchingAsync(CancellationToken cancellationToken)
    {
        await watchContainerEventsTokenSource.CancelAsync();
        client_.Dispose();
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
           await containerDetailsHub_.Clients.All.SendAsync(HubRouting.ContainerUpdateMethod(environment_.Id), @event, cancellationToken);
        }

    }

    private Data.ContainerStatus MapStatus(string status) => status.ToLower() switch
    {
        { } d when d.StartsWith("exited") => Data.ContainerStatus.Exited,
        { } d when d.StartsWith("created") => Data.ContainerStatus.Created,
        { } d when d.StartsWith("restarting") => Data.ContainerStatus.Restarted,
        { } d when d.StartsWith("running") => Data.ContainerStatus.Running,
        { } d when d.StartsWith("removing") => Data.ContainerStatus.Removing,
        { } d when d.StartsWith("paused") => Data.ContainerStatus.Paused,
        { } d when d.StartsWith("dead") => Data.ContainerStatus.Dead,
        _ => Data.ContainerStatus.NA
    };

    private ContainerDetailedModel ToContainer(ContainerInspectResponse response)
    {
        return new ContainerDetailedModel()
        {
            ContainerId = response.ID,
            ShortId = response.ID.Substring(0, 12),
            ContainerName = response.Name,
            Status = MapStatus(response.State.Status),
            Created = response.Created,
            ImageName = response.Image,
            EntryPoint = response.Config.Entrypoint.ToArray(),
            WorkingDirectory = response.Config.WorkingDir,
            Command = response.Config.Cmd.ToArray(),
            User = response.Config.User,
            Environment = ConvertEnvironment(),
            Labels = new (response.Config.Labels),
            RestartCount = response.RestartCount,
            RestartPolicy = response.HostConfig.RestartPolicy.Name,
            Mounts = response.Mounts.ToArray(),
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

    private ContainerModel ToContainer(ContainerListResponse response)
    {
        return new ContainerModel()
        {
            ContainerId = response.ID,
            ShortId = response.ID.Substring(0, 12),
            ContainerName = response.Names.First(), // todo,
            Status = MapStatus(response.State),
            Created = response.Created,
            ImageName = response.Image,
            Ports = response.Ports.Select(p => (p.PublicPort, p.PublicPort)).ToArray(), //todo
        };
    }

    public class DockerMessage // (events.Message)
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("Type")]
        public string Type { get; set; }

        [JsonPropertyName("Action")]
        public string Action { get; set; }

        [JsonPropertyName("Actor")]
        public DockerActor Actor { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("timeNano")]
        public long TimeNano { get; set; }
    }

    public class DockerActor // (events.Actor)
    {
        [JsonPropertyName("ID")]
        public string ID { get; set; }

        [JsonPropertyName("Attributes")]
        public IDictionary<string, string> Attributes { get; set; }
    }

}

public static class MultiplexedStreamExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_stream")]
    static extern ref Stream stream_(MultiplexedStream stream); 

    public static Stream GetStream(this MultiplexedStream stream) => stream_(stream);
}