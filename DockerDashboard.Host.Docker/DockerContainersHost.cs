using System.Runtime.CompilerServices;
using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;
using DockerDashboard.Shared.Messaging;
using DockerDashboard.Shared.Services;

namespace DockerDashboard.Host.Docker;

internal class DockerContainersHost : IDockerHostContainerManager
{
    private readonly IDockerClient _client;
    private readonly IMessageBus _containerDetailsHub;
    private readonly IDockerHostImageManager _imageManager;
    private readonly DockerEnvironment _environment;

    public DockerContainersHost(
        IDockerClient client,
        IMessageBus containerDetailsHub,
        IDockerHostImageManager imageManager,
        DockerEnvironment environment)
    {
        _client = client;
        _containerDetailsHub = containerDetailsHub;
        _imageManager = imageManager;
        _environment = environment;
    }

    public async IAsyncEnumerable<ContainerLog> GetContainerLogsAsync(string containerId,
        DateTimeOffset? since,
        DateTimeOffset? until,
        long? top,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var parameters = new ContainerLogsParameters
        {
            ShowStderr = true,
            ShowStdout = true,
            Timestamps = true,
            Tail = "100"
        };
        if (since.HasValue)
        {
            parameters.Since = since.Value.ToUnixTimeSeconds().ToString();
        }

        if (until.HasValue)
        {
            parameters.Until = until.Value.ToUnixTimeSeconds().ToString();
        }

        if (top.HasValue)
        {
            parameters.Tail = top.Value.ToString();
        }

        using var multiStream = await _client.Containers.GetContainerLogsAsync(containerId, false, parameters, cancellationToken);

        var underlyingStream = multiStream.GetStream();
        using var reader = new StreamReader(underlyingStream, Encoding.UTF8, false);
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;

            var log = ParseLog(line.AsSpan());
            if (log is not null)
            {
                yield return log;
            }
        }

        static ContainerLog? ParseLog(ReadOnlySpan<char> line)
        {
            var start = 0;

            while (start < line.Length && line[start] is '\u0001' or '\u0000' or '\0')
            {
                start++;
            }

            if (start < line.Length)
            {
                start++;
            }

            if (start < line.Length)
            {
                line = line.Slice(start);
            }

            //2014-09-16T06:17:46.000000000Z
            var format = "yyyy-MM-ddTHH:mm:ss.sssssssssZ";

            if (format.Length < line.Length)
            {
                var maybeTimestamp = line.Slice(0, format.Length);
                if (DateTime.TryParse(maybeTimestamp, out var timeStamp))
                {
                    return new(timeStamp, new string(line.Slice(format.Length)));
                }
            }

            return null;
        }
    }

    public async IAsyncEnumerable<ContainerModel> GetContainersAsync(string? beforeContainerId, long? take, [EnumeratorCancellation] CancellationToken cancellationToken)
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
            yield return container.ToContainer();
        }
    }

    
    public async Task<ContainerModel?> TryGetContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        var data = await _client.Containers.ListContainersAsync(new ContainersListParameters()
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>(){ {"id", new Dictionary<string, bool>() { {containerId, true } } } },
            All=true,
        }, cancellationToken);

        return data.First().ToContainer();
    }

    public async Task<ContainerDetailedModel?> TryGetContainerDetailsAsync(string containerId, CancellationToken cancellationToken)
    {
        var data = await _client.Containers.InspectContainerAsync(containerId, cancellationToken);
        if (data == null)
        {
            return null;
        }
        var result = data.ToContainer();
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

    public async Task RecreateContainerAsync(string containerId, bool pullImage, IProgress<ProgressEvent> progress, CancellationToken cancellationToken)
    {
        var data = await _client.Containers.InspectContainerAsync(containerId, cancellationToken);
        if (data == null)
        {
            await _containerDetailsHub.SendToAllAsync(HubRouting.ContainerDestroyMethod(_environment.Id), new DestroyContainerEvent(containerId), cancellationToken);
            return;
        }

        if (pullImage)
        {
           await _imageManager.PullImageAsync(data.Config.Image, progress, cancellationToken);
        }

        await DeleteContainerAsync(containerId, cancellationToken);
        await _client.Containers.CreateContainerAsync(new CreateContainerParameters(data.Config),cancellationToken);
    }
}