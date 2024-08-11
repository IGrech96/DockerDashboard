
using System.ComponentModel.DataAnnotations;

namespace DockerDashboard.Shared.Data;

public class ContainerModel
{
    [Key]
    public string ContainerId { get; set; }

    public string ShortId { get; set; }

    public string ContainerName { get; set; }

    public string ImageName { get; set; }

    public ContainerStatus Status { get; set; }

    public DateTime Created { get; set; }

    public (ushort localPort, ushort containerPort)[] Ports { get; set; }

    public void Populate(ContainerModel other)
    {
        ContainerId = other.ContainerId;
        ShortId = other.ShortId;
        ContainerName = other.ContainerName;
        ImageName = other.ImageName;
        Status = other.Status;
        Created = other.Created;
        Ports = other.Ports;
    }
}

public enum ContainerStatus
{
    Created,

    Starting,

    Running,

    Stoping,

    Exited,

    Restarted,

    Removing,

    Dead,

    Paused,

    NA,
    
}