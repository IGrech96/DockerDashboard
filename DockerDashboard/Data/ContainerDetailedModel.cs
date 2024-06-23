using Docker.DotNet.Models;

namespace DockerDashboard.Data;

public class ContainerDetailedModel : ContainerModel
{
    public string[] EntryPoint { get; set; }

    public string WorkingDirectory { get; set; }

    public string[] Command { get; set; }

    public string User { get; set; }

    public Dictionary<string, string> Environment { get; set; }

    public Dictionary<string, string> Labels { get; set; }

    public long RestartCount { get; set; }

    public RestartPolicyKind RestartPolicy { get; set; }

    public MountPoint[] Mounts { get; set; }
}