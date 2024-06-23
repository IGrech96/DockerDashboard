namespace DockerDashboard.Hubs;

public static class HubRouting
{
    public static string ContainerUpdateMethod(long environemntId) => $"{environemntId}:container_update";

    public static string ContainerLogMethod(long environemntId) => $"{environemntId}:container_log";
}
