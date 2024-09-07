using DockerDashboard.Hubs;
using DockerDashboard.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DockerDashboard.Progress;

public class HubProgress : IProgress<ProgressEvent>
{
    private readonly string _progressTrackId;
    private readonly IHubContext<ContainerDetailsHub> _hub;

    public HubProgress(string progressTrackId, IHubContext<ContainerDetailsHub> hub)
    {
        _progressTrackId = progressTrackId;
        _hub = hub;
    }

    public void Report(ProgressEvent value)
    {
        _hub.Clients.All.SendAsync(_progressTrackId, value);
    }
}