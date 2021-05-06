using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SIO.Infrastructure.Events;

namespace SIO.Google.Synthesiser.Events
{
    public interface IEventManager
    {
        Task ProcessAsync<T>(StreamId streamId, T @event, CancellationToken cancellationToken = default) where T : IEvent;
    }
}
