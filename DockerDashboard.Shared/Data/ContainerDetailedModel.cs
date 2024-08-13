using System.Runtime.Serialization;

namespace DockerDashboard.Shared.Data;

public class ContainerDetailedModel : ContainerModel
{
    public string[] EntryPoint { get; set; }

    public string WorkingDirectory { get; set; }

    public string[] Command { get; set; }

    public string User { get; set; }

    public KeyValue[] Environment { get; set; }

    public KeyValue[] Labels { get; set; }

    public long RestartCount { get; set; }

    public RestartPolicy RestartPolicy { get; set; }

    public MountPoint[] Mounts { get; set; }
}

public record KeyValue
{
    public KeyValue()
    {

    }
    public KeyValue(string Key, string Value)
    {
        this.Key = Key;
        this.Value = Value;
    }

    public string Key { get; init; }
    public string Value { get; init; }

    public void Deconstruct(out string Key, out string Value)
    {
        Key = this.Key;
        Value = this.Value;
    }
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