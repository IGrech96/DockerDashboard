namespace DockerDashboard.Services.Environment;

public interface IDockerEnvironmentManager
{
    IAsyncEnumerable<DockerEnvironment> GetAllEnvironmentsAsync(CancellationToken cancellationToken);
}

public record struct DockerEnvironment
{
    public long Id { get; set; }

    public string Name { get; set; }
}