using DockerDashboard.Shared.Services;
using DockerDashboard.Shared.Services.Environment;
using DockerDashboard.Ui;
using DockerDashboard.Ui.Clients;
using DockerDashboard.Ui.Logging;
using DockerDashboard.Ui.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using Radzen;
using Simple.OData.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient(ClientCategory.Backend, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped<IDockerEnvironmentManager, DockerEnvironmentManager>();
builder.Services.AddScoped<IDockerHostManager, DockerHostManager>();
builder.Services.AddSingleton<IPageDetailsNotificationService, SimplePageDetailsNotificationService>();
builder.Services.AddKeyedScoped<ODataClient>(ClientCategory.Backend, (provider, o) =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient(ClientCategory.Backend);
    var oDataClientSettings = new ODataClientSettings(httpClient, new Uri("odata", UriKind.Relative))
    {
        OnTrace = OnTrace
    };

    void OnTrace(string arg1, object[] arg2)
    {
    }

    var client = new ODataClient(oDataClientSettings);
    
    return client;
});

var userNotificationsProvider = new UserNotificationsProvider();
builder.Services.AddSingleton<IUserNotificationsPublisher>(userNotificationsProvider);
builder.Services.AddSingleton<IUserNotificationsConsumer>(userNotificationsProvider);


builder.Services.AddLogging(b => b.AddProvider(new UiLoggerProvider(userNotificationsProvider)));

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(ClientCategory.Backend));

await builder.Build().RunAsync();
