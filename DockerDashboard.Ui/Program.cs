using DockerDashboard.Shared.Services.DockerHost;
using DockerDashboard.Shared.Services.Environment;
using DockerDashboard.Ui;
using DockerDashboard.Ui.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient("DockerDashboard.Ui.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped<IDockerEnvironmentManager, DockerEnvironmentManager>();
builder.Services.AddScoped<IDockerHostManager, DockerHostManager>();
builder.Services.AddSingleton<IPageDetailsNotificationService, SimplePageDetailsNotificationService>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DockerDashboard.Ui.ServerAPI"));

await builder.Build().RunAsync();
