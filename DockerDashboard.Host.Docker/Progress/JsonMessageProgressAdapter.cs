using Docker.DotNet.Models;
using DockerDashboard.Shared.Hubs;

namespace DockerDashboard.Host.Docker.Progress;

public class JsonMessageProgressAdapter : IProgress<JSONMessage>
{
    private readonly IProgress<ProgressEvent> _source;

    public JsonMessageProgressAdapter(IProgress<ProgressEvent> source)
    {
        _source = source;
    }

    public void Report(JSONMessage value)
    {
        var data = new ProgressEvent()
        {
            Message = value.ProgressMessage ?? value.ErrorMessage ?? value.Status,
            Timestamp = value.Time != DateTime.MinValue ? value.Time : DateTime.Now,
        };

        _source.Report(data);
    }
}