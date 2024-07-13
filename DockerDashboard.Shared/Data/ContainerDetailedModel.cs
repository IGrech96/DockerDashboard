using System.Runtime.Serialization;

namespace DockerDashboard.Shared.Data;

public class ContainerDetailedModel : ContainerModel
{
    public string[] EntryPoint { get; set; }

    public string WorkingDirectory { get; set; }

    public string[] Command { get; set; }

    public string User { get; set; }

    public Dictionary<string, string> Environment { get; set; }

    public Dictionary<string, string> Labels { get; set; }

    public long RestartCount { get; set; }

    public RestartPolicy RestartPolicy { get; set; }

    public MountPoint[] Mounts { get; set; }
}

public enum RestartPolicy
{
    Undefined,

    No,

    Always,

    OnFailure,

    UnlessStopped
}

public class MountPoint
{
    public string Type { get; set; }

    public string Name { get; set; }

    public string Source { get; set; }

    public string Destination { get; set; }

    public string Driver { get; set; }

    public string Mode { get; set; }

    public bool RW { get; set; }

    public string Propagation { get; set; }
}