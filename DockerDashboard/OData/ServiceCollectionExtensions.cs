using Microsoft.AspNetCore.OData;

namespace DockerDashboard.OData;

public static class ServiceCollectionExtensions
{
    public static IMvcBuilder AddDockerDashboardOData(this IMvcBuilder builder)
    {
        return builder.AddOData((opt, sp) => opt.AddRouteComponents("odata", sp.GetRequiredService<IEdmBuilder>().GetEdmModel()).SkipToken());
    }
}