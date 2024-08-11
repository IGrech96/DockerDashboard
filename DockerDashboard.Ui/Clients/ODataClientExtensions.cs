using System.Runtime.CompilerServices;
using Simple.OData.Client;

namespace DockerDashboard.Ui.Clients;

public static class ODataClientExtensions
{
    public static async IAsyncEnumerable<TModel> FindEntriesAllPagesAsync<TModel>(
        this IBoundClient<TModel> client,
        [EnumeratorCancellation] CancellationToken cancellationToken) 
        where TModel : class
    {
        var annotations = new ODataFeedAnnotations();

        var data = await client.FindEntriesAsync(annotations, cancellationToken);
        foreach (var dockerEnvironment in data)
        {
            yield return dockerEnvironment;
        }

        while (annotations.NextPageLink != null)
        {
            data = await client.FindEntriesAsync(annotations.NextPageLink, annotations, cancellationToken);

            foreach (var dockerEnvironment in data)
            {
                yield return dockerEnvironment;
            }
        }
    }
}