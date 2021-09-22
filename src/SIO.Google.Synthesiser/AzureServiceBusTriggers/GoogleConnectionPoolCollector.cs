using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SIO.Domain.Translations.Events;
using SIO.Google.Credentials.Connections;
using SIO.Google.Synthesiser.Projections;
using SIO.Infrastructure.Azure.ServiceBus.Messages;
using SIO.Infrastructure.Connections.Pooling;
using SIO.Infrastructure.Events;

namespace SIO.Google.Synthesiser.Triggers
{
    class GoogleConnectionPoolCollector
    {
        public const string Name = "sio-google-connection-pool-collector";
        private readonly IEventContextFactory _eventContextFactory;
        private readonly IConnectionPool<GoogleConnection> _googleConnectionPool;
        private readonly ILogger<GoogleConnectionPoolCollector> _logger;
        private readonly IInMemoryProjector<TranslationProjection> _inMemoryProjector;

        public GoogleConnectionPoolCollector(IEventContextFactory eventContextFactory,
            IConnectionPool<GoogleConnection> googleConnectionPool,
            ILogger<GoogleConnectionPoolCollector> logger,
            IInMemoryProjector<TranslationProjection> inMemoryProjector)
        {
            if (eventContextFactory == null)
                throw new ArgumentNullException(nameof(eventContextFactory));
            if (googleConnectionPool == null)
                throw new ArgumentNullException(nameof(googleConnectionPool));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (inMemoryProjector == null)
                throw new ArgumentNullException(nameof(inMemoryProjector));

            _eventContextFactory = eventContextFactory;
            _googleConnectionPool = googleConnectionPool;
            _inMemoryProjector = inMemoryProjector;
            _logger = logger;
        }

        [FunctionName(Name)]
        public async Task ExecuteAsync([ServiceBusTrigger("%Topic%", "%ConnectionPoolCollectorSubscription%", Connection = "AzureServiceBus")] Message message, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(GoogleSynthesiserManager)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (message.Label == nameof(TranslationCharactersProcessed) || message.Label == nameof(TranslationFailed))
            {
                var context = _eventContextFactory.CreateContext(message);
                var streamId = StreamId.From(context.StreamId);
                var projection = await _inMemoryProjector.ProjectAsync(streamId, context.Payload);

                if (projection.Stopped && projection.TranslationType == Domain.Documents.Events.TranslationType.Google)
                    _googleConnectionPool.ReleaseConnection(streamId);
            }
        }
    }
}
