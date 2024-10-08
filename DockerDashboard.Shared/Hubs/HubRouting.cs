﻿namespace DockerDashboard.Shared.Hubs;

public static class HubRouting
{
    public static string ContainerUpdateMethod(long environemntId) => $"{environemntId}:container_update";

    public static string ContainerCreateMethod(long environemntId) => $"{environemntId}:container_create";

    public static string ContainerDestroyMethod(long environemntId) => $"{environemntId}:container_destroy";

    public static string ContainerLogMethod(long environemntId) => $"{environemntId}:container_log";
}
