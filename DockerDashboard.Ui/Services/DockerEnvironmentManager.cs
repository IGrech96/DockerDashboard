using System.Runtime.CompilerServices;
using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Services.Environment;
using DockerDashboard.Ui.Clients;
using Simple.OData.Client;
using static DockerDashboard.Ui.Logging.Events;

namespace DockerDashboard.Ui.Services
{
    public class DockerEnvironmentManager : IDockerEnvironmentManager
    {
        private readonly ODataClient _client;
        private readonly ILogger<UserMarker> _logger;

        public DockerEnvironmentManager(
            [FromKeyedServices(ClientCategory.Backend)]ODataClient client,
            ILogger<UserMarker> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async IAsyncEnumerable<DockerEnvironment> GetAllEnvironmentsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            IEnumerable<DockerEnvironment> data = [];
            try
            {
                data = await _client.For<DockerEnvironment>().FindEntriesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(Logging.Events.Environments, ex, "Can not retrieve list of environments.");
            }
            //finally
            //{
            //    _logger.LogInformation(Logging.Events.Environments, "Test");
            //}

            foreach (var dockerEnvironment in data)
            {
                yield return dockerEnvironment;
            }
        }
    }
}
