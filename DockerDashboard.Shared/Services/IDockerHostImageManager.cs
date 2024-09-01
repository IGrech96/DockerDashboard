using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DockerDashboard.Shared.Data;

namespace DockerDashboard.Shared.Services;

public interface IDockerHostImageManager
{
    IAsyncEnumerable<ImageModel> GetImagesAsync(CancellationToken cancellationToken);
}