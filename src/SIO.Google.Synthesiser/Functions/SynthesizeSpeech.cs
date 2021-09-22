using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.TextToSpeech.V1;
using Grpc.Auth;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SIO.Domain.Translations.Events;
using SIO.Google.Credentials.Connections;
using SIO.Infrastructure.Connections.Pooling;
using SIO.Infrastructure.Events;
using SIO.Infrastructure.Files;

namespace SIO.Google.Synthesiser.Functions
{
    public class SynthesizeSpeech
    {
        public const string Name = "sio-google-synthesiser-synthesize-speech";
        private readonly ILogger<SynthesizeSpeech> _logger;
        private readonly IFileClient _fileClient;
        private readonly Events.IEventManager _eventManager;
        private readonly IConnectionPool<GoogleConnection> _googleConnectionPool;

        public SynthesizeSpeech(ILogger<SynthesizeSpeech> logger,
            IFileClient fileClient,
            Events.IEventManager eventManager,
            IConnectionPool<GoogleConnection> googleConnectionPool)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (fileClient == null)
                throw new ArgumentNullException(nameof(fileClient));
            if (eventManager == null)
                throw new ArgumentNullException(nameof(eventManager));
            if (googleConnectionPool == null)
                throw new ArgumentNullException(nameof(googleConnectionPool));

            _logger = logger;
            _fileClient = fileClient;
            _eventManager = eventManager;
            _googleConnectionPool = googleConnectionPool;
        }

        [FunctionName(Name)]
        public async Task ExecuteAsync([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var request = context.GetInput<SynthesizeSpeechRequest>();
            try
            {
                if (request.Delay > 0)
                    await context.CreateTimer(context.CurrentUtcDateTime.AddMinutes(request.Delay), CancellationToken.None);

                var streamId = StreamId.From(request.StreamId);

                var connection = _googleConnectionPool.GetConnection(streamId);

                var builder = new TextToSpeechClientBuilder();
                builder.ChannelCredentials = connection.Credential.ToChannelCredentials();
                var client = builder.Build();

                var response = await client.SynthesizeSpeechAsync(
                    input: new SynthesisInput
                    {
                        Text = request.Text
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

                var fileName = $"{request.Subject}_{request.Version}.mp3";

                using (var ms = new MemoryStream(response.AudioContent.ToByteArray()))
                {
                    await _fileClient.UploadAsync(fileName, request.UserId, ms);
                }

                var translationCharactersProcessed = new TranslationCharactersProcessed(request.Subject, request.Version, fileName, request.Text.Length);
                await _eventManager.ProcessAsync(streamId, translationCharactersProcessed);
            }
            catch(Exception e)
            {
                var translationFailed = new TranslationFailed(request.Subject, request.Version, e.Message);
                await _eventManager.ProcessAsync(StreamId.From(request.StreamId), translationFailed);
            }
        }
    }
}
