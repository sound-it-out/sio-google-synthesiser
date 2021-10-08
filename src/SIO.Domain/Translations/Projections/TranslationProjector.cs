using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SIO.Infrastructure.Events;
using SIO.Infrastructure.Projections;

namespace SIO.Google.Synthesiser.Projections
{
    internal sealed class TranslationProjector : IInMemoryProjector<TranslationProjection>
    {
        private readonly IEventStore _eventStore;
        private readonly ILogger<ProjectionManager<TranslationProjection>> _logger;

        public TranslationProjector(IEventStore eventStore,
            ILogger<ProjectionManager<TranslationProjection>> logger)
        {
            _eventStore = eventStore;
            _logger = logger;
        }

        public async Task<TranslationProjection> ProjectAsync(StreamId streamId, params IEvent[] events)
        {
            var projection = new TranslationProjection(_logger);
            var exisitngEvents = await _eventStore.GetEventsAsync(streamId);

            foreach (var @event in exisitngEvents.Select(c => c.Payload).Concat(events))
                await projection.HandleAsync(@event);

            return projection;
        }
    }
}
