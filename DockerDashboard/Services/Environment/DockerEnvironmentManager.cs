using DockerDashboard.Hubs;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services.Environment;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using DockerDashboard.Services.DockerHost;
using DockerDashboard.Shared;
using DockerDashboard.Shared.Services;

namespace DockerDashboard.Services.Environment;

public class DockerEnvironmentManager : IDockerEnvironmentManager, IHostedService
{
    private readonly ConcurrentDictionary<long, IDockerHost> _hosts = new ();
    private readonly IHubContext<ContainerDetailsHub> _hubContex;
    private readonly DockerEnvironmentProvider[] _providers;

    public DockerEnvironmentManager(IHubContext<ContainerDetailsHub> hubContex, IEnumerable<DockerEnvironmentProvider> providers)
    {
        _hubContex = hubContex;
        _providers = providers.ToArray();
    }

    public IAsyncEnumerable<DockerEnvironment> GetAllEnvironmentsAsync(CancellationToken cancellationToken)
    {
        return _providers.Select(p => p.Environment).ToAsyncEnumerable();
    }



    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var startTasks = new List<Task>();
        foreach (var provider in _providers)
        {
            _hosts[provider.Environment.Id] = provider.HostFactory();

            startTasks.Add(_hosts[provider.Environment.Id].StartWatchingAsync(cancellationToken));
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

    public IDockerHost GetHost(long environment)
    {
        return _hosts[environment];
    }
}