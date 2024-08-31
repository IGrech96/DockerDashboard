using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerDashboard.Shared.Messaging;

public interface IMessageBus
{
    public Task SendToAllAsync<T>(string method, T message, CancellationToken cancellationToken);
}