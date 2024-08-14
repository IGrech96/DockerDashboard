namespace DockerDashboard.Shared.Data;

public enum RestartPolicy
{
    Undefined,

    No,

    Always,

    OnFailure,

    UnlessStopped
}