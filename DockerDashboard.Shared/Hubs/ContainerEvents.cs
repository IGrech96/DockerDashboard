using System.Text.Json;
using DockerDashboard.Shared.Data;
using System.Text.Json.Serialization;

namespace DockerDashboard.Shared.Hubs;

[JsonDerivedType(typeof(DestroyContainerEvent), 0)]
[JsonDerivedType(typeof(CreateContainerEvent), 1)]
[JsonDerivedType(typeof(UpdateContainerEvent), 2)]
[JsonDerivedType(typeof(ContainerLogEvent), 3)]
public abstract class ContainerEvent(string containerId)
{
    public string ContainerId { get; set; } = containerId;
}

[method: JsonConstructor]
public class DestroyContainerEvent(string containerId) : ContainerEvent(containerId)
{
}

[method: JsonConstructor]
public class CreateContainerEvent(string containerId, ContainerModel container) : ContainerEvent(containerId) 
{
    public ContainerModel Container { get; set; } = container;
}

[method: JsonConstructor]
public class UpdateContainerEvent(string containerId, ContainerModel container) : ContainerEvent(containerId)
{
    public ContainerModel Container { get; set; } = container;
}

[method: JsonConstructor]
public class ContainerLogEvent(string containerId, string message) : ContainerEvent(containerId)
{
    public string Message {get;set; } = message;
}


public class ContainerEventConverter : JsonConverter<ContainerEvent>
{
    public override ContainerEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, ContainerEvent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("_t");
        writer.WriteStringValue(value.GetType().FullName);
        JsonSerializer.Serialize(writer, value);
        writer.WriteEndObject();
    }
}
