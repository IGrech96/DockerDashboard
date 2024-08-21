using DockerDashboard.Hubs;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services.Environment;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using DockerDashboard.Services.DockerHost;

namespace DockerDashboard.Services.Environment;

public class DockerEnvironmentManager : IDockerEnvironmentManager, IHostedService
{
    private ConcurrentDictionary<long, DockerHost.IDockerHost> _hosts = new ();
    private IHubContext<ContainerDetailsHub> _hubContex;

    public DockerEnvironmentManager(IHubContext<ContainerDetailsHub> hubContex)
    {
        _hubContex = hubContex;
    }

    public IAsyncEnumerable<DockerEnvironment> GetAllEnvironmentsAsync(CancellationToken cancellationToken)
    {
        return new DockerEnvironment[]
        {
            new() { Id = 1, Name = "Local" },
            new () {Id = 2, Name = "Demo" }
        }.ToAsyncEnumerable();
    }



    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var startTasks = new List<Task>();
        await foreach (var env in GetAllEnvironmentsAsync(cancellationToken))
        {
            IDockerHost host = env.Id == 2
                ? new DemoDockerHost(_hubContex, env)
                : new DockerHost.DockerHost(_hubContex, env);
            _hosts[env.Id] = host;

            startTasks.Add(host.StartWatchingAsync(cancellationToken));
        }

        await Task.WhenAll(startTasks);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var stopTasks = new List<Task>();
        foreach (var host in _hosts.Values)
        {
            stopTasks.Add(host.StopWatchingAsync(cancellationToken));
        }

        await Task.WhenAll(stopTasks);
    }

    public DockerHost.IDockerHost GetHost(long environment)
    {
        return _hosts[environment];
    }
}