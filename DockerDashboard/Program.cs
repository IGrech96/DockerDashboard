using System.Text.Json;
using DockerDashboard.Host.Demo;
using DockerDashboard.Host.Docker;
using DockerDashboard.Hubs;
using DockerDashboard.OData;
using DockerDashboard.Services.DockerHost;
using DockerDashboard.Services.Environment;
using DockerDashboard.Services.Registry;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;
using DockerDashboard.Shared.Messaging;
using DockerDashboard.Shared.Services;
using DockerDashboard.Shared.Services.Environment;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IDockerHostManager, DockerHostManager>();
builder.Services.AddSignalR().AddJsonProtocol(jo =>
{
    //jo.PayloadSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
});

builder.Services.AddScoped<IPageDetailsNotificationService, SimplePageDetailsNotificationService>();
builder.Services.AddSingleton<DockerEnvironmentManager>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DockerEnvironmentManager>());
builder.Services.AddSingleton<IDockerEnvironmentManager>(sp => sp.GetRequiredService<DockerEnvironmentManager>());
builder.Services.AddSingleton<IEdmBuilder, DefaultEdmBuilder>();
builder.Services.AddTransient<IMessageBus, HubContextMessageBus>();
builder.Services.AddSingleton<IDockerRegistryManager, DockerRegistryManager>();

builder.Services.Configure<DockerRegistryOptions>(builder.Configuration.GetSection(nameof(DockerRegistryOptions)));

builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });                
    });


builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(MetadataController).Assembly)
    .AddDockerDashboardOData();

builder.Services
    .AddLocalEnvironment()
    .AddDemoEnvironment();

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
app.UseODataRouteDebug();

app.MapRazorPages();
app.MapFallbackToFile("index.html");

//app.MapFallbackToPage("/_Host");

//GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => JsonSerializer.Create(jsonSerializerSettings));

app.MapHub<ContainerDetailsHub>("/containerDetailsHub");


app.MapControllers();


//var mng = app.Services.GetRequiredService<DockerEnvironmentManager>();
//await mng.StartAsync(CancellationToken.None);
//var svc = app.Services.GetRequiredService<IDockerHostManager>();

//await svc.GetImageManager(1).PullImageAsync("ivang2896044/receiptstorage:latest", CancellationToken.None);
app.Run();
