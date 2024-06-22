namespace DockerDashboard.Services.Environment;

public interface IDockerEnvironmentManager
{
    IAsyncEnumerable<DockerEnvironemnt> GetAllEnvironmentsAsync(CancellationToken cancellationToken);
}

public record struct DockerEnvironemnt
{
    public long Id { get; set; }

    public string Name { get; set; }
}