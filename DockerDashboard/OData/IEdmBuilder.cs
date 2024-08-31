using DockerDashboard.Shared.Data;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace DockerDashboard.OData;

public interface IEdmBuilder
{
    IEdmModel GetEdmModel();
}

public class DefaultEdmBuilder : IEdmBuilder
{
    public IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new();
        builder.EntitySet<ContainerModel>("Containers");
        builder.EntitySet<DockerEnvironment>("DockerEnvironments");
        builder.EntitySet<ContainerDetailedModel>("Details").EntityType.HasKey(k => k.ContainerId);
        builder.EntitySet<ContainerLog>("Logs");

        var containerDetailsFunction = builder.EntityType<ContainerModel>().Function("Details");
        containerDetailsFunction.Parameter<long>("environment");
        containerDetailsFunction.ReturnsFromEntitySet<ContainerDetailedModel>("Details");

        builder.EntityType<ContainerModel>().Action("Stop").Parameter<long>("environment").Required();
        builder.EntityType<ContainerModel>().Action("Start").Parameter<long>("environment").Required();
        builder.EntityType<ContainerModel>().Action("Pause").Parameter<long>("environment").Required();
        builder.EntityType<ContainerModel>().Action("Restart").Parameter<long>("environment").Required();
        
        var recreateAction = builder.EntityType<ContainerModel>().Action("Recreate");
        recreateAction.Parameter<long>("environment").Required();
        recreateAction.Parameter<bool>("pullImage").Optional();

        var logsFunction = builder.EntityType<ContainerModel>().Function("Logs");
        logsFunction.Parameter<long>("environment").Required();
        logsFunction.Parameter<DateTimeOffset?>("until").Optional();
        logsFunction.Parameter<DateTimeOffset?>("since").Optional();
        logsFunction.ReturnsFromEntitySet<ContainerLog>("Logs");

        return builder.GetEdmModel();
    }
}