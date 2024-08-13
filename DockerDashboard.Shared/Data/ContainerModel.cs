
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

    public ContainerPort[] Ports { get; set; } = [];

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

public record ContainerPort
{
    public ContainerPort()
    {

    }
    public ContainerPort(ushort LocalPort, ushort Port)
    {
        this.LocalPort = LocalPort;
        this.Port = Port;
    }

    public ushort LocalPort { get; init; }
    public ushort Port { get; init; }

    public void Deconstruct(out ushort Key, out ushort Value)
    {
        Key = this.LocalPort;
        Value = this.Port;
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