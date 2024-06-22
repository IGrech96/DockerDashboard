namespace DockerDashboard.Services.Environment;

public class DockerEnvironmentManager : IDockerEnvironmentManager, IHostedService
{
    public async IAsyncEnumerable<DockerEnvironemnt> GetAllEnvironmentsAsync(CancellationToken cancellationToken)
    {
        yield return new DockerEnvironemnt() { Id = 1, Name ="Local" };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
    }
}