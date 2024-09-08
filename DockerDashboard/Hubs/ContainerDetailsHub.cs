using DockerDashboard.Shared.Hubs;
using DockerDashboard.Shared.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace DockerDashboard.Hubs;

public class ContainerDetailsHub : Hub
{
}

public class HubContextMessageBus : IMessageBus
{
    private readonly IHubContext<ContainerDetailsHub> _hubContext;

    public HubContextMessageBus(IHubContext<ContainerDetailsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendToAllAsync<T>(string method, T message, CancellationToken cancellationToken)
    {
        return _hubContext.Clients.All.SendAsync(method, message, cancellationToken);
    }
}