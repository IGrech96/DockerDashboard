using DockerDashboard.Data;
using Microsoft.AspNetCore.SignalR;

namespace DockerDashboard.Hubs;

public class ContainerDetailsHub : Hub
{
    public async Task SendMessage(ContainerDetailedModel model)
    {
        await Clients.All.SendAsync(model.ContainerId, model);
    }
}