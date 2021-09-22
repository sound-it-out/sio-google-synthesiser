using System;
using System.Threading;
using System.Threading.Tasks;
using SIO.Infrastructure;
using SIO.Infrastructure.Events;

namespace SIO.Google.Synthesiser.Events
{
    internal sealed class DefaultEventManager : IEventManager
    {
        private readonly IEventStore _eventStore;
        private readonly IEventBusPublisher _eventBusPublisher;

        public DefaultEventManager(IEventStore eventStore,
            IEventBusPublisher eventBusPublisher)
        {
            if (eventStore == null)
                throw new ArgumentNullException(nameof(eventStore));
            if (eventBusPublisher == null)
                throw new ArgumentNullException(nameof(eventBusPublisher));

            _eventStore = eventStore;
            _eventBusPublisher = eventBusPublisher;
        }

        public async Task ProcessAsync<TEvent>(StreamId streamId, TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            var context = new EventContext<IEvent>(streamId: streamId, @event: @event, correlationId: null, causationId: null, @event.Timestamp, actor: Actor.From("unknown"));
            var notification = new EventNotification<TEvent>(streamId: StreamId.New(), @event: @event, correlationId: null, causationId: null, timestamp: DateTimeOffset.UtcNow, userId: null);
            await _eventStore.SaveAsync(streamId, new IEventContext<IEvent>[] { context }, cancellationToken);
            await _eventBusPublisher.PublishAsync(notification, cancellationToken);
        }
    }
}
