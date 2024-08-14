using System.Runtime.Serialization;

namespace DockerDashboard.Shared.Data;

public class ContainerDetailedModel : ContainerModel
{
    public string[] EntryPoint { get; set; } = [];

    public string? WorkingDirectory { get; set; }

    public string[] Command { get; set; } = [];

    public string? User { get; set; }

    public KeyValue[] Environment { get; set; } = [];

    public KeyValue[] Labels { get; set; } = [];

    public long RestartCount { get; set; }

    public RestartPolicy RestartPolicy { get; set; }

    public MountPoint[] Mounts { get; set; } = [];

    public override void Populate(ContainerModel other)
    {
        base.Populate(other);
        if (other is ContainerDetailedModel otherDetailedModel)
        {
            EntryPoint = otherDetailedModel.EntryPoint.ToArray();
            Command = otherDetailedModel.Command.ToArray();
            WorkingDirectory = otherDetailedModel.WorkingDirectory;
            User = otherDetailedModel.User;
            Environment = otherDetailedModel.Environment.ToArray();
            Labels = otherDetailedModel.Labels.ToArray();
            RestartCount = otherDetailedModel.RestartCount;
            RestartPolicy = otherDetailedModel.RestartPolicy;
            Mounts = otherDetailedModel.Mounts.ToArray();
        }
    }
}