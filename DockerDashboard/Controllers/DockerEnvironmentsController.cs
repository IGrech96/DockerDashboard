using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services.Environment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace DockerDashboard.Controllers
{
    public class DockerEnvironmentsController : ODataController
    {
        private readonly IDockerEnvironmentManager _dockerEnvironmentManager;

        public DockerEnvironmentsController(IDockerEnvironmentManager dockerEnvironmentManager)
        {
            _dockerEnvironmentManager = dockerEnvironmentManager;
        }

        [HttpGet()]
        public IAsyncEnumerable<DockerEnvironment> GetDockerEnvironments(CancellationToken cancellationToken)
        {
            return _dockerEnvironmentManager.GetAllEnvironmentsAsync(cancellationToken);
        }
    }
}
