namespace DockerDashboard.Ui.Logging;

public static class Events
{
    public struct UserMarker { }

    public static readonly EventId Environments = new EventId(0, "Docker Environments");

    public static readonly EventId Containers = new EventId(1, "Docker Containers");

    public static readonly EventId Images = new EventId(2, "Docker Images");
}