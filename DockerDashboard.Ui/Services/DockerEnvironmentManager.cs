using System.Runtime.CompilerServices;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services.Environment;
using DockerDashboard.Ui.Clients;
using Simple.OData.Client;

namespace DockerDashboard.Ui.Services
{
    public class DockerEnvironmentManager : IDockerEnvironmentManager
    {
        private readonly ODataClient _client;

        public DockerEnvironmentManager([FromKeyedServices(ClientCategory.Backend)]ODataClient client)
        {
            _client = client;
        }

        public async IAsyncEnumerable<DockerEnvironment> GetAllEnvironmentsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var data = await _client.For<DockerEnvironment>().FindEntriesAsync(cancellationToken);

            foreach (var dockerEnvironment in data)
            {
                yield return dockerEnvironment;
            }
        }
    }
}
