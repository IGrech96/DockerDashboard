using System.Runtime.CompilerServices;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Messaging;
using DockerDashboard.Shared.Services;

namespace DockerDashboard.Host.Docker;

public class DockerImagesHost : IDockerHostImageManager
{
    private readonly IDockerClient _client;
    private readonly IMessageBus _containerDetailsHub;
    private readonly DockerEnvironment _environment;

    public DockerImagesHost(IDockerClient client, IMessageBus containerDetailsHub, DockerEnvironment environment)
    {
        _client = client;
        _containerDetailsHub = containerDetailsHub;
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
}