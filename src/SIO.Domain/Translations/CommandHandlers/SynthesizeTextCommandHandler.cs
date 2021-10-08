using Clipboard;
using Google.Cloud.TextToSpeech.V1;
using Grpc.Auth;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using SIO.Domain.Translations.Commands;
using SIO.Domain.Translations.Events;
using SIO.Google.Credentials.Connections;
using SIO.Infrastructure;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Connections.Pooling;
using SIO.Infrastructure.Events;
using SIO.Infrastructure.Files;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SIO.Domain.Translations.CommandHandlers
{
    internal sealed class SynthesizeTextCommandHandler : ICommandHandler<SynthesizeTextCommand>
    {
        private readonly ILogger<SynthesizeTextCommandHandler> _logger;
        private readonly IFileClient _fileClient;
        private readonly IConnectionPool<GoogleConnection> _googleConnectionPool;
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly IEventStore _eventStore;

        public SynthesizeTextCommandHandler(ILogger<SynthesizeTextCommandHandler> logger,
            IFileClient fileClient,
            IConnectionPool<GoogleConnection> googleConnectionPool,
            IContentTypeProvider contentTypeProvider,
            IEventStore eventStore)
        {
            if(logger == null)
                throw new ArgumentNullException(nameof(logger));
            if(fileClient == null)
                throw new ArgumentNullException(nameof(fileClient));
            if(googleConnectionPool == null)
                throw new ArgumentNullException(nameof(googleConnectionPool));
            if(contentTypeProvider == null)
                throw new ArgumentNullException(nameof(contentTypeProvider));
            if(eventStore == null)
                throw new ArgumentNullException(nameof(eventStore));

            _logger = logger;
            _fileClient = fileClient;
            _googleConnectionPool = googleConnectionPool;
            _contentTypeProvider = contentTypeProvider;
            _eventStore = eventStore;
        }

        public async Task ExecuteAsync(SynthesizeTextCommand command, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(SynthesizeTextCommandHandler)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            using (var ms = new MemoryStream())
            {
                var file = _fileClient.DownloadAsync(command.FileName, command.Actor, ms, cancellationToken);
                ms.Position = 0;

                if (!_contentTypeProvider.TryGetContentType(command.FileName, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                using (var textExtractor = TextExtractor.Open(ms, contentType))
                {
                    var text = await textExtractor.ExtractAsync();

                    var streamId = StreamId.From(command.CorrelationId);

                    var connection = _googleConnectionPool.GetConnection(streamId);

                    var builder = new TextToSpeechClientBuilder();
                    builder.ChannelCredentials = connection.Credential.ToChannelCredentials();
                    var client = builder.Build();

                    var response = await client.SynthesizeSpeechAsync(
                        input: new SynthesisInput
                        {
                            Text = text
                        },
                        voice: new VoiceSelectionParams
                        {
                            Name = ""
                        },
                        audioConfig: new AudioConfig
                        {
                            AudioEncoding = AudioEncoding.Mp3
                        }
                    );

                    var fileName = $"{command.Subject}_{command.ProcessedVersion}.mp3";

                    using (var stream = new MemoryStream(response.AudioContent.ToByteArray()))
                    {
                        await _fileClient.UploadAsync(fileName, command.Actor, ms);
                    }

                    var translationCharactersProcessed = new TranslationCharactersProcessed(
                        subject: command.Subject,
                        version: command.ProcessedVersion,
                        fileName: fileName,
                        charactersProcessed: text.Length
                    );

                    var context = new EventContext<TranslationCharactersProcessed>(
                        streamId: command.CorrelationId,
                        @event: translationCharactersProcessed,
                        correlationId: command.CorrelationId,
                        causationId: CausationId.From(command.Id),
                        timestamp: translationCharactersProcessed.Timestamp,
                        actor: command.Actor);

                    await _eventStore.SaveAsync(streamId, new IEventContext<IEvent>[] { context }, cancellationToken);
                }
            }            
        }
    }
}
