namespace DockerDashboard.Shared.Hubs;

public class ProgressEvent
{
    public DateTime? Timestamp { get; set; }

    public string? Message { get; set; }
}