using Clipboard;
using Google.Cloud.TextToSpeech.V1;
using Grpc.Auth;
using Microsoft.AspNetCore.StaticFiles;
using SIO.Domain.Documents.Aggregates;
using SIO.Domain.GoogleSynthesizes.Aggregates;
using SIO.Domain.Translations.Aggregates;
using SIO.Domain.Translations.Commands;
using SIO.EntityFrameworkCore.DbContexts;
using SIO.Google.Credentials.Connections;
using SIO.Infrastructure;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Connections.Pooling;
using SIO.Infrastructure.Domain;
using SIO.Infrastructure.EntityFrameworkCore.DbContexts;
using SIO.Infrastructure.Extensions;
using SIO.Infrastructure.Files;
using System.Collections.Concurrent;

namespace SIO.Domain.Translations.CommandHandlers
{
    internal class StartTranslationCommandHandler : ICommandHandler<StartTranslationCommand>
    {
        private readonly IAggregateRepository<SIOGoogleSynthesizerStoreDbContext> _googleSynthesizerAggregateRepository;
        private readonly IAggregateRepository<SIOStoreDbContext> _storeAggregateRepository;        
        private readonly IFileClient _fileClient;
        private readonly IConnectionPool<GoogleConnection> _googleConnectionPool;
        private readonly IContentTypeProvider _contentTypeProvider;

        public StartTranslationCommandHandler(IAggregateRepository<SIOGoogleSynthesizerStoreDbContext> googleSynthesizerAggregateRepository,
            IAggregateRepository<SIOStoreDbContext> storeAggregateRepository,
            IFileClient fileClient,
            IConnectionPool<GoogleConnection> googleConnectionPool,
            IContentTypeProvider contentTypeProvider)
        {
            if (googleSynthesizerAggregateRepository is null)
                throw new ArgumentNullException(nameof(googleSynthesizerAggregateRepository));
            if (storeAggregateRepository is null)
                throw new ArgumentNullException(nameof(storeAggregateRepository));
            if (fileClient is null)
                throw new ArgumentNullException(nameof(fileClient));
            if (googleConnectionPool is null)
                throw new ArgumentNullException(nameof(googleConnectionPool));
            if (contentTypeProvider is null)
                throw new ArgumentNullException(nameof(contentTypeProvider));

            _googleSynthesizerAggregateRepository = googleSynthesizerAggregateRepository;
            _storeAggregateRepository = storeAggregateRepository;
            _fileClient = fileClient;
            _googleConnectionPool = googleConnectionPool;
            _contentTypeProvider = contentTypeProvider;
        }

        public async Task ExecuteAsync(StartTranslationCommand command, CancellationToken cancellationToken = default)
        {
            var connectionId = ConnectionId.From(command.Subject);

            var googleSynthesis = await _googleSynthesizerAggregateRepository.GetAsync<GoogleSynthesize, GoogleSynthesizeState>(command.Subject, cancellationToken);
            var googleSynthesisState = googleSynthesis.GetState();

            var translation = await _storeAggregateRepository.GetAsync<Translation, TranslationState>(command.Subject, cancellationToken);
            var translationState = translation.GetState();

            var document = await _storeAggregateRepository.GetAsync<Document, DocumentState>(translationState.DocumentSubject, cancellationToken);
            var documentState = document.GetState();

            try
            {
                if (!googleSynthesisState.SynthesizeProcesses.Any())
                {
                    using var ms = new MemoryStream();
                    await _fileClient.DownloadAsync($"{documentState.Subject}{Path.GetExtension(documentState.FileName)}", documentState.User, ms, cancellationToken);
                    ms.Position = 0;

                    if (!_contentTypeProvider.TryGetContentType(documentState.FileName, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }

                    using var textExtractor = TextExtractor.Open(ms, contentType);
                    var text = await textExtractor.ExtractAsync();
                    var chunks = text.ChunkWithDelimeters(5000, '.', '!', '?', ')', '"', '}', ']').ToArray();
                    translation.Start(chunks.Sum(c => c.Length));

                    var i = 0;
                    
                    foreach (var chunk in chunks)
                    {
                        using var s = new MemoryStream();
                        using var sw = new StreamWriter(s);
                        var processSubject = Subject.New();                
                        await sw.WriteAsync(chunk);
                        await sw.FlushAsync();
                        s.Position = 0;
                        await _fileClient.UploadAsync($"{processSubject}.txt", documentState.User, s, cancellationToken);

                        googleSynthesis.QueueProcess(i++, processSubject: processSubject);
                    }                        

                    await _googleSynthesizerAggregateRepository.SaveAsync(googleSynthesis, command, cancellationToken: cancellationToken);
                    googleSynthesisState = googleSynthesis.GetState();
                }

                await Task.WhenAll(googleSynthesisState.SynthesizeProcesses.Where(sp => !sp.Processed).Select(sp => 
                    ProcessTextChunkasync(
                        command: command,
                        googleSynthesis: googleSynthesis,
                        translation: translation,
                        documentState: documentState,
                        synthesizeProcess: sp,
                        connectionId: connectionId,
                        cancellationToken: cancellationToken
                    )
                ));

                _googleConnectionPool.ReleaseConnection(connectionId);

                using var audioStream = await GetAudioStream(googleSynthesis, documentState, cancellationToken);

                await _fileClient.UploadAsync($"{documentState.Subject}.mp3", documentState.User, audioStream, cancellationToken);                
                await Task.WhenAll(
                    googleSynthesisState.SynthesizeProcesses.Select(sp => _fileClient.DeleteAsync($"{sp.Subject}.txt", documentState.User, cancellationToken))
                    .Concat(new Task[] { _fileClient.DeleteAsync($"{documentState.Subject}.txt", documentState.User, cancellationToken) })  
                );

                googleSynthesis.Succeed();
                translation.Succeed();
            }
            catch (Exception ex)
            {                
                var message = $"{nameof(StartTranslationCommandHandler)}.{nameof(StartTranslationCommandHandler.ExecuteAsync)} - {ex.Message}";
                translation.Fail(message);
                googleSynthesis.Fail(message);              
            }
            finally
            {
                _googleConnectionPool.ReleaseConnection(connectionId);
                await _googleSynthesizerAggregateRepository.SaveAsync(googleSynthesis, command, cancellationToken: cancellationToken);
                await _storeAggregateRepository.SaveAsync(translation, command, cancellationToken: cancellationToken);
            }
        }

        private async Task<Stream> GetAudioStream(GoogleSynthesize googleSynthesize, DocumentState documentState, CancellationToken cancellationToken)
        {
            var bytes = new ConcurrentBag<KeyValuePair<int, byte[]>>();

            await Task.WhenAll(googleSynthesize.GetState().SynthesizeProcesses.Select(async sp =>
            {
                using var ms = new MemoryStream();
                await _fileClient.DownloadAsync($"{sp.Subject}.mp3", documentState.User, ms, cancellationToken);
                bytes.Add(new KeyValuePair<int, byte[]>(sp.Order, ms.ToArray()));
            }));

            return new MemoryStream(CombineArrays(bytes.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToArray()));
        }

        private byte[] CombineArrays(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        private async Task ProcessTextChunkasync(StartTranslationCommand command,
            GoogleSynthesize googleSynthesis,
            Translation translation,
            DocumentState documentState,
            SynthesizeProcess synthesizeProcess,
            ConnectionId connectionId,
            CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream();            
            using var textExtractor = TextExtractor.Open(ms, "text/plain");

            await _fileClient.DownloadAsync($"{synthesizeProcess.Subject}.txt", documentState.User, ms, cancellationToken);
            var text = await textExtractor.ExtractAsync();

            var googleConnection = _googleConnectionPool.GetConnection(connectionId);
            var builder = new TextToSpeechClientBuilder();
            builder.ChannelCredentials = googleConnection.Credential.ToChannelCredentials();
            var client = builder.Build();

            var response = await client.SynthesizeSpeechAsync(
                input: new SynthesisInput
                {
                    Text = text
                },
                voice: new VoiceSelectionParams
                {
                    Name = documentState.TranslationOptionSubject,
                    LanguageCode = "en-GB"
                },
                audioConfig: new AudioConfig
                {
                    AudioEncoding = AudioEncoding.Mp3
                }
            );

            using var stream = new MemoryStream(response.AudioContent.ToByteArray());
            await _fileClient.UploadAsync($"{synthesizeProcess.Subject}.mp3", documentState.User, stream, cancellationToken);            

            translation.Process(text.Length);
            googleSynthesis.SucceedProcess(synthesizeProcess.Subject);

            await _googleSynthesizerAggregateRepository.SaveAsync(googleSynthesis, command, cancellationToken: cancellationToken);
            await _storeAggregateRepository.SaveAsync(translation, command, cancellationToken: cancellationToken);            
        }
    }
}
