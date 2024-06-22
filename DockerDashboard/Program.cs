using DockerDashboard.Data;
using DockerDashboard.Hubs;
using DockerDashboard.Services;
using DockerDashboard.Services.DockerHost;
using DockerDashboard.Services.Environment;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<IDockerHostManager, DockerHostManager>();
builder.Services.AddRadzenComponents();
builder.Services.AddSignalR();

builder.Services.AddScoped<IPageDetailsNotificationService, SimplePageDetailsNotificationService>();
builder.Services.AddSingleton<DockerEnvironmentManager>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DockerEnvironmentManager>());
builder.Services.AddSingleton<IDockerEnvironmentManager>(sp => sp.GetRequiredService<DockerEnvironmentManager>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapHub<ContainerDetailsHub>("/containerDetailsHub");

app.Run();
