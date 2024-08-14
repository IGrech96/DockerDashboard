namespace DockerDashboard.Shared.Data;

public record ContainerPort
{
    public ContainerPort()
    {

    }
    public ContainerPort(ushort LocalPort, ushort Port)
    {
        this.LocalPort = LocalPort;
        this.Port = Port;
    }

    public ushort LocalPort { get; init; }
    public ushort Port { get; init; }

    public void Deconstruct(out ushort Key, out ushort Value)
    {
        Key = this.LocalPort;
        Value = this.Port;
    }
}