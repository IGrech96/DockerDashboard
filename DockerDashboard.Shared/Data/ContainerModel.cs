
using System.ComponentModel.DataAnnotations;

namespace DockerDashboard.Shared.Data;

public class ContainerModel
{
    [Key]
    public required string ContainerId { get; set; }

    public string ShortId => ContainerId.Substring(0, Math.Min(12, ContainerId.Length));

    public required string ContainerName { get; set; }

    public required string ImageName { get; set; }

    public ContainerStatus Status { get; set; }

    public DateTime Created { get; set; }

    public ContainerPort[] Ports { get; set; } = [];

    public virtual void Populate(ContainerModel other)
    {
        ContainerId = other.ContainerId;
        ContainerName = other.ContainerName;
        ImageName = other.ImageName;
        Status = other.Status;
        Created = other.Created;
        Ports = other.Ports;
    }
}