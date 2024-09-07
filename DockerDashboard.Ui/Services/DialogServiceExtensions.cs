using DockerDashboard.Shared.Hubs;
using DockerDashboard.Ui.Components;
using Radzen;

namespace DockerDashboard.Ui.Services;

public static class DialogServiceExtensions
{

    public static Task<dynamic> OpenProgressDialogAsync(
        this Radzen.DialogService service, 
        string title,
        ProgressDialog.ObservableAction action)
    {
        return service.OpenAsync<ProgressDialog>(
            title,
            new () { ["Action"] = action });
    }
}