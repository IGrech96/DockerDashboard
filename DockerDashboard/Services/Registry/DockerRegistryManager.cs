using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services;
using Microsoft.Extensions.Options;

namespace DockerDashboard.Services.Registry;

public class DockerRegistryManager : IDockerRegistryManager
{
    private readonly IOptionsMonitor<DockerRegistryOptions> _options;

    public DockerRegistryManager(IOptionsMonitor<DockerRegistryOptions> options)
    {
        _options = options;
    }

    public Task<DockerRegistry?> TryGetRegistryAsync(string imageName, CancellationToken cancellationToken)
    {
        var currentRegistries = _options.CurrentValue;

        var result = currentRegistries
            .Registries
            .OrderByDescending(_ => _.Name.Length)
            .FirstOrDefault(_ => imageName.StartsWith(_.Name));

        if (result == null)
        {
            return Task.FromResult<DockerRegistry?>(null);
        }

        return Task.FromResult<DockerRegistry?>(new DockerRegistry() { Name = result.Name });
    }

    public Task<(string username, string password)?> TryGetCredentailsAsync(DockerRegistry registry, CancellationToken cancellationToken)
    {
        
        var currentRegistries = _options.CurrentValue;

        var result = currentRegistries
            .Registries
            .OrderByDescending(_ => _.Name.Length)
            .FirstOrDefault(_ => registry.Name == _.Name);

        if (result == null || result.User == null || result.Token == null)
        {
            return Task.FromResult<(string username, string password)?>(null);
        }

        return Task.FromResult<(string username, string password)?>((result.User, result.Token));
    }
}

public class DockerRegistryOptions
{
    public DockerRegistryItem[] Registries { get; set; } = [];
}

public class DockerRegistryItem
{
    public required string Name { get; set; }

    public string? User { get; set; }

    public string? Token { get; set; }
}