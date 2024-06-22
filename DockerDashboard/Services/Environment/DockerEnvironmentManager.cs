using DockerDashboard.Hubs;
using DockerDashboard.Services.DockerHost;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DockerDashboard.Services.Environment;

public class DockerEnvironmentManager : IDockerEnvironmentManager, IHostedService
{
    private ConcurrentDictionary<long, DockerHost.DockerHost> _hosts = new ();
    private IHubContext<ContainerDetailsHub> _hubContex;

    public DockerEnvironmentManager(IHubContext<ContainerDetailsHub> hubContex)
    {
        _hubContex = hubContex;
    }

    public async IAsyncEnumerable<DockerEnvironment> GetAllEnvironmentsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return new DockerEnvironment() { Id = 1, Name ="Local" };
    }



    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var startTasks = new List<Task>();
        await foreach (var env in GetAllEnvironmentsAsync(cancellationToken))
        {
            var host = new DockerHost.DockerHost(_hubContex, env);
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

    public DockerHost.DockerHost GetHost(long environment)
    {
        return _hosts[environment];
    }
}