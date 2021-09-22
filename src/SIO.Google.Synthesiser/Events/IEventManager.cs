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
        Task ProcessAsync<TEvent>(StreamId streamId, TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
    }
}
