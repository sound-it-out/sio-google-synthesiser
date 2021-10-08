using Clipboard;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using SIO.Domain.Translations.Commands;
using SIO.Domain.Translations.Events;
using SIO.Infrastructure;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Events;
using SIO.Infrastructure.Extensions;
using SIO.Infrastructure.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SIO.Domain.Translations.CommandHandlers
{
    internal sealed class QueueTranslationCommandHandler : ICommandHandler<QueueTranslationCommand>
    {
        private readonly ILogger<QueueTranslationCommandHandler> _logger;
        private readonly IFileClient _fileClient;
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly IEventStore _eventStore;

        public QueueTranslationCommandHandler(ILogger<QueueTranslationCommandHandler> logger,
            IFileClient fileClient,
            IContentTypeProvider contentTypeProvider,
            IEventStore eventStore)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (fileClient == null)
                throw new ArgumentNullException(nameof(fileClient));
            if (contentTypeProvider == null)
                throw new ArgumentNullException(nameof(contentTypeProvider));
            if (eventStore == null)
                throw new ArgumentNullException(nameof(eventStore));

            _logger = logger;
            _fileClient = fileClient;
            _contentTypeProvider = contentTypeProvider;
            _eventStore = eventStore;
        }

        public async Task ExecuteAsync(QueueTranslationCommand command, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(QueueTranslationCommandHandler)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    await _fileClient.DownloadAsync(command.FileName, command.Actor, ms, cancellationToken);
                    ms.Position = 0;

                    if (!_contentTypeProvider.TryGetContentType(command.FileName, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }

                    using (var textExtractor = TextExtractor.Open(ms, contentType))
                    {
                        var text = await textExtractor.ExtractAsync();
                        var chunks = text.ChunkWithDelimeters(5000, '.', '!', '?', ')', '"', '}', ']').ToArray();

                        var translationStarted = new TranslationStarted(command.Subject, command.Version, chunks.Sum(c => c.Length), chunks.Length);
                        var context = new EventContext<TranslationStarted>(
                            streamId: command.CorrelationId,
                            @event: translationStarted,
                            correlationId: command.CorrelationId,
                            causationId: CausationId.From(command.Id),
                            timestamp: translationStarted.Timestamp,
                            actor: command.Actor);

                        await _eventStore.SaveAsync(StreamId.From(command.CorrelationId), new IEventContext<IEvent>[] { context }, cancellationToken);

                        var index = 1;
                        var timeout = 0;
                        var textGroups = chunks.Chunk(30).ToArray();

                        var synthesisTasks = textGroups.Select(async (textGroup, i) => {
                            var @event = new TranslationSynthesiseQueued(textGroups.Length + i + 1, $"{command.FileName}-text-{i + 1}", command.Subject, translationStarted.Version + i + 1);
                            using (var stream = new MemoryStream())
                            using (var sw = new StreamWriter(stream))
                            {
                                foreach(var text in textGroup)
                                    await sw.WriteAsync(text);

                                await sw.FlushAsync();
                                stream.Position = 0;

                                await _fileClient.UploadAsync(@event.FileName, command.Actor, stream, cancellationToken);
                            }                            

                            return @event;
                        }).ToArray();

                        await Task.WhenAll(synthesisTasks);

                        var translationSynthesiseQueuedEventContexts = synthesisTasks.Select((st, i) => new EventContext<TranslationSynthesiseQueued>(
                                streamId: command.CorrelationId,
                                @event: st.Result,
                                correlationId: command.CorrelationId,
                                causationId: CausationId.From(command.Id),
                                timestamp: st.Result.Timestamp,
                                actor: command.Actor,
                                scheduledPublication: DateTimeOffset.UtcNow.AddMinutes(5 + i)));

                        await _eventStore.SaveAsync(StreamId.From(command.CorrelationId), translationSynthesiseQueuedEventContexts, cancellationToken);
                    }
                }
            }
            catch (Exception e)
            {
                //var translationFailed = new TranslationFailed(request.Subject, request.Version, e.Message);
                //await _eventManager.ProcessAsync(StreamId.From(request.StreamId), translationFailed);
            }
        }
    }
}
