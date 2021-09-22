using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SIO.Domain.Documents.Events;
using SIO.Domain.Translations.Events;
using SIO.Google.Synthesiser.Functions;
using SIO.Infrastructure.Azure.ServiceBus.Messages;
using SIO.Infrastructure.Events;

namespace SIO.Google.Synthesiser.AzureServiceBusTriggers
{
    class GoogleSynthesiserManager
    {
        public const string Name = "sio-google-synthesiser";
        private readonly IEventContextFactory _eventContextFactory;
        private readonly Events.IEventManager _eventManager;
        private readonly IProcessText _processText;
        private readonly ILogger<GoogleSynthesiserManager> _logger;

        public GoogleSynthesiserManager(IEventContextFactory eventContextFactory,
            Events.IEventManager eventManager,
            IProcessText processText,
            ILogger<GoogleSynthesiserManager> logger)
        {
            if (eventContextFactory == null)
                throw new ArgumentNullException(nameof(eventContextFactory));
            if (eventManager == null)
                throw new ArgumentNullException(nameof(eventManager));
            if (processText == null)
                throw new ArgumentNullException(nameof(processText));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _eventContextFactory = eventContextFactory;
            _eventManager = eventManager;
            _processText = processText;
            _logger = logger;
        }

        [FunctionName(Name)]
        public async Task ExecuteAsync([ServiceBusTrigger("%Topic%", "%Subscription%", Connection = "AzureServiceBus")] Message message,
            [DurableClient] IDurableOrchestrationClient client, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(GoogleSynthesiserManager)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (message.Label == nameof(DocumentUploaded))
            {
                var context = (IEventContext<DocumentUploaded>)_eventContextFactory.CreateContext(message);

                if (context.Payload.TranslationType != TranslationType.Google)
                    return;

                var translationQueuedEvent = new TranslationQueued(
                    subject: context.Payload.Subject,
                    version: 2
                );

                var streamId = StreamId.From(context.StreamId);

                await _eventManager.ProcessAsync(streamId, translationQueuedEvent, cancellationToken);

                try
                {
                    var request = new ProcessTextRequest
                    {
                        StreamId = streamId,
                        FileName = $"{translationQueuedEvent.Subject}{Path.GetExtension(context.Payload.FileName)}",
                        Subject = translationQueuedEvent.Subject,
                        UserId = context.Payload.User,
                        Version = translationQueuedEvent.Version + 1
                    };

                    await _processText.ExecuteAsync(request, client, cancellationToken);
                }
                catch (Exception e)
                {
                    var translationFailed = new TranslationFailed(translationQueuedEvent.Subject, translationQueuedEvent.Version, e.Message);
                    await _eventManager.ProcessAsync(streamId, translationFailed);
                }                
            }
        }
    }
}
