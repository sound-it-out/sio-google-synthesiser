using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using SIO.Domain.Documents.Events;
using SIO.Domain.Translations.Commands;
using SIO.Domain.Translations.Events;
using SIO.Google.Synthesiser.Projections;
using SIO.Infrastructure;
using SIO.Infrastructure.Azure.ServiceBus.Messages;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Events;

namespace SIO.Google.Synthesiser.Triggers
{
    public class GoogleSynthesizerTrigger
    {
        const string AzureServiceBusTrigger = nameof(GoogleSynthesizerTrigger) + "." + nameof(TriggerAzureServiceBusAsync);

        private readonly ILogger<GoogleSynthesizerTrigger> _logger;
        private readonly IEventContextFactory _eventContextFactory;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IInMemoryProjector<TranslationProjection> _projector;

        public GoogleSynthesizerTrigger(ILogger<GoogleSynthesizerTrigger> logger,
            IEventContextFactory eventContextFactory,
            ICommandDispatcher commandDispatcher,
            IInMemoryProjector<TranslationProjection> projector)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (eventContextFactory == null)
                throw new ArgumentNullException(nameof(eventContextFactory));
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));
            if (projector == null)
                throw new ArgumentNullException(nameof(projector));

            _logger = logger;
            _eventContextFactory = eventContextFactory;
            _commandDispatcher = commandDispatcher;
            _projector = projector;
        }

        [Function(AzureServiceBusTrigger)]
        public async Task TriggerAzureServiceBusAsync([ServiceBusTrigger("%Topic%", "%Subscription%", Connection = "AzureServiceBus")] Message message)
        {
            if (message.Label == nameof(DocumentUploaded))
            {
                var context = (IEventContext<DocumentUploaded>)_eventContextFactory.CreateContext(message);
                await Handle(context);
            }

            if (message.Label == nameof(TranslationSynthesiseQueued))
            {
                var context = (IEventContext<TranslationSynthesiseQueued>)_eventContextFactory.CreateContext(message);
                await Handle(context);
            }
        }

        private async Task Handle(IEventContext<DocumentUploaded> context)
        {
            if (context.Payload.TranslationType != TranslationType.Google)
                return;

            await _commandDispatcher.DispatchAsync(new QueueTranslationCommand(
                fileName: context.Payload.FileName,
                subject: context.Payload.Subject,
                correlationId: CorrelationId.From(context.StreamId),
                version: context.Payload.Version + 1,
                actor: context.Actor
            ));
        }

        private async Task Handle(IEventContext<TranslationSynthesiseQueued> context)
        {
            var translation = await _projector.ProjectAsync(StreamId.From(context.StreamId));

            if (translation.TranslationType != TranslationType.Google)
                return;

            await _commandDispatcher.DispatchAsync(new QueueTranslationCommand(
                fileName: context.Payload.FileName,
                subject: context.Payload.Subject,
                correlationId: CorrelationId.From(context.StreamId),
                version: context.Payload.Version + 1,
                actor: context.Actor
            ));
        }
    }
}
