using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using SIO.Domain.Documents.Events;
using SIO.Domain.Translations.Events;
using SIO.Google.Credentials.Connections;
using SIO.Google.Synthesiser.Projections;
using SIO.Infrastructure.Azure.ServiceBus.Messages;
using SIO.Infrastructure.Connections.Pooling;
using SIO.Infrastructure.Events;
using System;
using System.Threading.Tasks;

namespace SIO.Google.Synthesiser.Triggers
{
    public class GoogleConnectionPoolTrigger
    {
        const string AzureServiceBusTrigger = nameof(GoogleConnectionPoolTrigger) + "." + nameof(TriggerAzureServiceBusAsync);

        private readonly ILogger<GoogleSynthesizerTrigger> _logger;
        private readonly IEventContextFactory _eventContextFactory;
        private readonly IInMemoryProjector<TranslationProjection> _projector;
        private readonly IConnectionPool<GoogleConnection> _googleConnectionPool;

        public GoogleConnectionPoolTrigger(ILogger<GoogleSynthesizerTrigger> logger,
            IEventContextFactory eventContextFactory,
            IInMemoryProjector<TranslationProjection> projector,
            IConnectionPool<GoogleConnection> googleConnectionPool)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (eventContextFactory == null)
                throw new ArgumentNullException(nameof(eventContextFactory));
            if (projector == null)
                throw new ArgumentNullException(nameof(projector));
            if (googleConnectionPool == null)
                throw new ArgumentNullException(nameof(googleConnectionPool));

            _logger = logger;
            _eventContextFactory = eventContextFactory;            
            _projector = projector;
            _googleConnectionPool = googleConnectionPool;
        }

        [Function(AzureServiceBusTrigger)]
        public async Task TriggerAzureServiceBusAsync([ServiceBusTrigger("%Topic%", "%ConnectionPoolCollectorSubscription%", Connection = "AzureServiceBus")] Message message)
        {
            if (message.Label == nameof(TranslationCharactersProcessed) || message.Label == nameof(TranslationFailed))
            {                
                var context = _eventContextFactory.CreateContext(message);
                var streamId = StreamId.From(context.StreamId);
                var translation = await _projector.ProjectAsync(streamId);

                if (translation.TranslationType != TranslationType.Google || !translation.Stopped)
                    return;

                _logger.LogInformation($"Attempting to release {nameof(GoogleConnection)} for streamId: {streamId}");
                _googleConnectionPool.ReleaseConnection(streamId);
            }
        }
    }
}
