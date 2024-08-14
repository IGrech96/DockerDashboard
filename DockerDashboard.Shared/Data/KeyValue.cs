using System.Diagnostics.CodeAnalysis;

namespace DockerDashboard.Shared.Data;

public record KeyValue
{
    public KeyValue()
    {

    }

    [SetsRequiredMembers]
    public KeyValue(string Key, string Value)
    {
        this.Key = Key;
        this.Value = Value;
    }

    public required string Key { get; init; }
    public required string Value { get; init; }

    public void Deconstruct(out string Key, out string Value)
    {
        Key = this.Key;
        Value = this.Value;
    }
}