using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;

namespace DockerDashboard.Shared.Services;

public interface IDockerHostImageManager
{
    IAsyncEnumerable<ImageModel> GetImagesAsync(CancellationToken cancellationToken);

    Task PullImageAsync(string image, IProgress<ProgressEvent> progress, CancellationToken cancellationToken);

    Task<ImageModel?> TryGetImageAsync(string imageId, CancellationToken cancellationToken);

    Task DeleteImageAsync(string imageId, CancellationToken cancellationToken);
}