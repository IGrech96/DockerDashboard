using DockerDashboard.Data;
using Microsoft.AspNetCore.SignalR;

namespace DockerDashboard.Hubs;

public class ContainerDetailsHub : Hub
{
}

public abstract class ContainerEvent(string containerId)
{
    public string ContainerId { get; set; } = containerId;
}

public class DestroyContainerEvent(string containerId) : ContainerEvent(containerId)
{
}

public class CreateContainerEvent(string containerId, ContainerModel container) : ContainerEvent(containerId) 
{
    public ContainerModel Container { get; set; } = container;
}

public class UpdateContainerEvent(string containerId, ContainerModel container) : ContainerEvent(containerId) 
{
    public ContainerModel Container { get; set; } = container;
}