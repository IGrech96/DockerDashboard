namespace DockerDashboard.Data;

public interface IDockerHostManager
{
    Task<ContainersSnapshot> GetContainersSnapshot();

    IAsyncEnumerable<ContainerModel> GetContainers(Guid snapshotId, int startIndex, int count);

    IAsyncEnumerable<ContainerModel> GetContainers(Guid snapshotId);
}

public class ContainersSnapshot
{
    public int TotalCount { get; set; }

    public Guid SnapshotId { get; set; }
}