using DockerDashboard.Shared.Data;
using DockerDashboard.Shared.Hubs;
using DockerDashboard.Ui.Components;
using Radzen;
using Radzen.Blazor;

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

    public enum ReCreateContainerDialogResult
    {
        Cancel,

        Recreate,

        RecreateWithNewImage
    }

    public static async Task<ReCreateContainerDialogResult> OpenRecreateContainerDialogAsync(
        this DialogService service,
        ContainerModel container)
    {
        Dictionary<string, object> data = new()
        {
            ["Container"] = container,
            ["PullImage"] = false
        };
        var result = await service.OpenAsync<RecreateContainerDialog>("Re-Create container", data);

        if (result is ReCreateContainerDialogResult enumResult)
        {
            return enumResult;
        }

        return ReCreateContainerDialogResult.Cancel;
    }
}