using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DockerDashboard.Shared.Data;

public record ContainerLog
{
    [Key]
    public required DateTime Timestamp { get; init; }
    public required string Log { get; init; }

    public ContainerLog()
    {

    }

    [SetsRequiredMembers]
    public ContainerLog(DateTime timestamp, string log)
    {
        Timestamp = timestamp;
        Log = log;
    }
}