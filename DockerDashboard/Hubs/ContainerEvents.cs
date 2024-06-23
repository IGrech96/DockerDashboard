using DockerDashboard.Data;

namespace DockerDashboard.Hubs;

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

public class ContainerLogEvent(string containerId, string message) : ContainerEvent(containerId)
{
    public string Message {get;set; } = message;
}
