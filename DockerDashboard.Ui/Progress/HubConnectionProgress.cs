using DockerDashboard.Shared.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;

namespace DockerDashboard.Ui.Progress;

public class HubConnectionProgress : IAsyncDisposable
{
    private readonly IProgress<ProgressEvent> _source;

    private readonly HubConnection _connection;
    public HubConnectionProgress(NavigationManager navigation, IProgress<ProgressEvent> source)
    {
        _source = source;
        TrackId = Guid.NewGuid().ToString();
        _connection = new HubConnectionBuilder()
            //.AddJsonProtocol(op => op.PayloadSerializerOptions.)
            .WithUrl(navigation.ToAbsoluteUri("/containerDetailsHub"))
            .Build();
        
        _connection.On<ProgressEvent>(TrackId, OnProgressEvent);
    }

    private void OnProgressEvent(ProgressEvent obj)
    {
        _source.Report(obj);
    }

    public string TrackId { get; }

    public async Task StartAsync()
    {
        await _connection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}