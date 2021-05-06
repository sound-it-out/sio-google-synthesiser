using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SIO.Domain.Translations.Events;
using SIO.Infrastructure.Projections;

namespace SIO.Google.Synthesiser.Projections
{
    internal class TranslationProjection : ProjectionManager<TranslationProjection>, IProjection
    {
        public TranslationProjection(ILogger<ProjectionManager<TranslationProjection>> logger) : base(logger)
        {
            Handle<TranslationQueued>(HandleAsync);
            Handle<TranslationStarted>(HandleAsync);
            Handle<TranslationCharactersProcessed>(HandleAsync);
            Handle<TranslationFailed>(HandleAsync);
        }

        public bool Stopped => Failed || CharactersProcessed == TotalCharacters && (Version - 2) == ProcessCount;
        public bool Failed { get; private set; }
        public long CharactersProcessed { get; private set; }
        public long TotalCharacters { get; private set; }
        public long ProcessCount { get; private set; }
        public int Version { get; private set; }

        public Task HandleAsync(TranslationStarted @event)
        {
            TotalCharacters = @event.CharacterCount;
            ProcessCount = @event.ProcessCount;
            Version = @event.Version;
            return Task.CompletedTask;
        }

        public Task HandleAsync(TranslationQueued @event)
        {
            Version = @event.Version;
            return Task.CompletedTask;
        }

        public Task HandleAsync(TranslationCharactersProcessed @event)
        {
            CharactersProcessed += @event.CharactersProcessed;
            Version = @event.Version;
            return Task.CompletedTask;
        }

        public Task HandleAsync(TranslationFailed @event)
        {
            Failed = true;
            Version = @event.Version;
            return Task.CompletedTask;
        }

        public override Task ResetAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
