using DockerDashboard.Hubs;
using DockerDashboard.Services.DockerHost;
using DockerDashboard.Services.Environment;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services;
using DockerDashboard.Shared.Services.Environment;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IDockerHostManager, DockerHostManager>();
builder.Services.AddSignalR();

builder.Services.AddScoped<IPageDetailsNotificationService, SimplePageDetailsNotificationService>();
builder.Services.AddSingleton<DockerEnvironmentManager>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DockerEnvironmentManager>());
builder.Services.AddSingleton<IDockerEnvironmentManager>(sp => sp.GetRequiredService<DockerEnvironmentManager>());

builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });                
    });

static IEdmModel GetEdmModel()
{
    ODataConventionModelBuilder builder = new();
    builder.EntitySet<ContainerModel>("Containers");
    builder.EntitySet<DockerEnvironment>("DockerEnvironments");
    builder.EntitySet<ContainerDetailedModel>("Details").EntityType.HasKey(k => k.ContainerId);

    var containerDetailsFunction = builder.EntityType<ContainerModel>().Function("Details");
    containerDetailsFunction.Parameter<long>("environment");
    containerDetailsFunction.ReturnsFromEntitySet<ContainerDetailedModel>("Details");

    builder.EntityType<ContainerModel>().Action("Stop").Parameter<long>("environment").Required();
    builder.EntityType<ContainerModel>().Action("Start").Parameter<long>("environment").Required();
    builder.EntityType<ContainerModel>().Action("Pause").Parameter<long>("environment").Required();
    builder.EntityType<ContainerModel>().Action("Restart").Parameter<long>("environment").Required();

    return builder.GetEdmModel();
}


builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(MetadataController).Assembly)
    .AddOData(opt => opt.AddRouteComponents("odata", GetEdmModel()).SkipToken());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Error");s
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    });
    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
//app.UseODataRouteDebug();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

//app.MapFallbackToPage("/_Host");

app.MapHub<ContainerDetailsHub>("/containerDetailsHub");



app.UseEndpoints(endpoints => endpoints.MapControllers());
app.Run();
