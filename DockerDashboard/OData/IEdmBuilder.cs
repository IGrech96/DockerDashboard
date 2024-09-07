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
        BuildContainers(builder);


        var entitySet = builder.EntitySet<ImageModel>("Images");
        
        var pullAction = builder.EntityType<ImageModel>().Collection.Action("Pull");
        pullAction.Parameter<long>("environment").Required();
        pullAction.Parameter<string>("image").Required();
        pullAction.Parameter<string>("progressTrackId").Optional();


        return builder.GetEdmModel();
    }

    private static void BuildContainers(ODataConventionModelBuilder builder)
    {
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
        recreateAction.Parameter<string>("progressTrackId").Optional();

        var logsFunction = builder.EntityType<ContainerModel>().Function("Logs");
        logsFunction.Parameter<long>("environment").Required();
        logsFunction.Parameter<DateTimeOffset?>("until").Optional();
        logsFunction.Parameter<DateTimeOffset?>("since").Optional();
        logsFunction.ReturnsFromEntitySet<ContainerLog>("Logs");
    }
}