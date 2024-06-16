namespace DockerDashboard.Data;

public class ContainerModel
{
    public string ContainerId { get; set; }

    public string ShortId { get; set; }

    public string ContainerName { get; set; }

    public string ImageName { get; set; }

    public ContainerStatus Status { get; set; }

    public DateTime Created { get; set; }

    public (ushort localPort, ushort containerPort)[] Ports { get; set; }
}

public enum ContainerStatus
{
    Starting,

    Running,

    Stoping,

    Failed,

    Exited,

    Stoped,

    NA
}