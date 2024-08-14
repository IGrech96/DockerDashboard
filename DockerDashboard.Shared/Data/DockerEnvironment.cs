namespace DockerDashboard.Shared.Data;

public record DockerEnvironment
{
    public required long Id { get; set; }

    public required string Name { get; set; }
}