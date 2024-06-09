namespace DockerDashboard.Data;

public class Container
{
    public string ContainerId { get; set; }

    public string ContainerName { get; set; }

    public string ImageName { get; set; }

    public ContainerStatus Status { get; set; }

    public string Created { get; set; }

    public (int localPort, int containerPort)[] Ports { get; set; }
}

public enum ContainerStatus
{
    Starting,

    Running,

    Stoping,

    Failed,

    Stoped,
}