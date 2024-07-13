
public interface IPageDetailsNotificationService
{
    event Func<Task> PageDetailsOpen;
    event Func<Task> PageDetailsClose;

    Task OpenPageDetailsAsync();
    Task ClosePageDetailsAsync();

}

public class SimplePageDetailsNotificationService : IPageDetailsNotificationService
{
    public event Func<Task>? PageDetailsOpen;
    public event Func<Task>? PageDetailsClose;

    public async Task OpenPageDetailsAsync()
    {
        if (PageDetailsOpen != null)
        {
            await PageDetailsOpen();
        }
    }

    public async Task ClosePageDetailsAsync()
    {
        if (PageDetailsClose != null)
        {
            await PageDetailsClose();
        }
    }
}