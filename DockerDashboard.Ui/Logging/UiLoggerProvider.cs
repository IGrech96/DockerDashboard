using DockerDashboard.Ui.Services;
using Microsoft.Extensions.Logging.Abstractions;
using static DockerDashboard.Ui.Logging.Events;

namespace DockerDashboard.Ui.Logging;

public class UiLoggerProvider : ILoggerProvider
{
    private readonly IUserNotificationsPublisher _notificationsPublisher;

    //DockerDashboard.Ui.Logging.Events.UserMarker
    private readonly string _userCategory = typeof(UserMarker).FullName!.Replace("+",".");

    public UiLoggerProvider(IUserNotificationsPublisher notificationsPublisher)
    {
        _notificationsPublisher = notificationsPublisher;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (categoryName == _userCategory)
        {
            return new UiLogger(_notificationsPublisher);
        }
        return NullLogger.Instance;
    }

    private class UiLogger : ILogger
    {
        private readonly IUserNotificationsPublisher _notificationsPublisher;

        public UiLogger(IUserNotificationsPublisher notificationsPublisher)
        {
            _notificationsPublisher = notificationsPublisher;
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string title = logLevel.ToString();
            if (eventId == Events.Containers)
            {
                title = Events.Containers.Name!;
            }

            if (eventId == Events.Environments)
            {
                title = Events.Environments.Name!;
            }
            
            _notificationsPublisher.Notify(logLevel, title, formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Information;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }
    }
}