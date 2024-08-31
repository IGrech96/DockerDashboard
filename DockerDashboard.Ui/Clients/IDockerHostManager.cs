//using DockerDashboard.Shared.Data;

//namespace DockerDashboard.Ui.Clients;

//public interface IDockerHostManager
//{
//    IAsyncEnumerable<ContainerModel> GetContainersAsync(long environment, CancellationToken cancellationToken);

//    Task<ContainerModel> GetContainerAsync(long environment, string containerId, CancellationToken cancellationToken);

//    IAsyncEnumerable<string> GetContainerLogsAsync(long environment, string containerId, DateTimeOffset since,
//        DateTimeOffset until);

//    Task<ContainerDetailedModel> GetContainerDetails(long environment, string containerId);

//    Task PauseContainerAsync(long environment, string containerId);

//    Task StopContainerAsync(long environment, string containerId);

//    Task StartContainerAsync(long environment, string containerId);

//    Task DeleteContainerAsync(long environment, string containerId);
    
//    Task RestartContainerAsync(long environment, string containerId);
//}