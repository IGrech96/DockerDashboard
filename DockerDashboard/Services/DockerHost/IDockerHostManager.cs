﻿using DockerDashboard.Data;

namespace DockerDashboard.Services.DockerHost;

public interface IDockerHostManager
{
    IAsyncEnumerable<ContainerModel> GetContainers(long environment);

    Task<ContainerModel> GetContainerAsync(long environment, string containerId);
}