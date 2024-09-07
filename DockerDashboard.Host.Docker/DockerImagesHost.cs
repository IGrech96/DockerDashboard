using System.Runtime.CompilerServices;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerDashboard.Host.Docker.Progress;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;
using DockerDashboard.Shared.Messaging;
using DockerDashboard.Shared.Services;

namespace DockerDashboard.Host.Docker;

public class DockerImagesHost : IDockerHostImageManager
{
    private readonly IDockerClient _client;
    private readonly IMessageBus _containerDetailsHub;
    private readonly IDockerRegistryManager _registryManager;
    private readonly DockerEnvironment _environment;

    public DockerImagesHost(
        IDockerClient client,
        IMessageBus containerDetailsHub,
        IDockerRegistryManager registryManager,
        DockerEnvironment environment)
    {
        _client = client;
        _containerDetailsHub = containerDetailsHub;
        _registryManager = registryManager;
        _environment = environment;
    }

    public async IAsyncEnumerable<ImageModel> GetImagesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var images = await _client.Images.ListImagesAsync(new ImagesListParameters()
        {
            All = true,
        }, cancellationToken);

        foreach (var image in images)
        {
            foreach (var imageModel in image.ToImages())
            {
                yield return imageModel;
            }
        }
    }

    public async Task PullImageAsync(string image, IProgress<ProgressEvent> progress, CancellationToken cancellationToken)
    {
        var (imageName, tag) = DataModelExtensions.ParseImage(image);
        var imageParamters = new ImagesCreateParameters()
        {
            FromImage = imageName,
            Tag = tag
        };
        var authConfig = new AuthConfig();
        if (await _registryManager.TryGetRegistryAsync(imageName, cancellationToken) is { } registry &&
            await _registryManager.TryGetCredentailsAsync(registry, cancellationToken) is { } credentails)

        {
            authConfig.Username = credentails.username;
            authConfig.Password = credentails.password;
        }

        var progressAdapter = new JsonMessageProgressAdapter(progress);

        await _client.Images.CreateImageAsync(imageParamters, authConfig, progressAdapter, cancellationToken);

    }
}