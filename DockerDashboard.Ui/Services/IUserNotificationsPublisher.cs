namespace DockerDashboard.Ui.Services;

public interface IUserNotificationsPublisher
{
    void Notify(LogLevel level, string title, string message);
}

public interface IUserNotificationsConsumer
{
    public delegate void UserNofiticationConsumerDelegate(LogLevel level, string title, string message);

    event UserNofiticationConsumerDelegate Consumed;
}

public class UserNotificationsProvider : IUserNotificationsPublisher, IUserNotificationsConsumer
{
    public void Notify(LogLevel level, string title, string message)
    {
        OnConsumed(level, title, message);
    }

    public event IUserNotificationsConsumer.UserNofiticationConsumerDelegate? Consumed;

    protected virtual void OnConsumed(LogLevel level, string title, string message)
    {
        Consumed?.Invoke(level, title, message);
    }
}