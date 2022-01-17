using Microsoft.Extensions.DependencyInjection;
using SIO.Domain.GoogleSynthesizes.Aggregates;
using SIO.Domain.Translations.Aggregates;
using SIO.Domain.Translations.Commands;
using SIO.EntityFrameworkCore.DbContexts;
using SIO.Infrastructure;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Domain;
using SIO.Infrastructure.EntityFrameworkCore.DbContexts;
using SIO.Infrastructure.Events;
using SIO.Infrastructure.Processing;

namespace SIO.Domain.Translations.CommandHandlers
{
    internal sealed class QueueTranslationCommandHandler : ICommandHandler<QueueTranslationCommand>
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IAggregateRepository<SIOGoogleSynthesizerStoreDbContext> _googleSynthesizerAggregateRepository;
        private readonly IAggregateRepository<SIOStoreDbContext> _storeAggregateRepository;
        private readonly IAggregateFactory _aggregateFactory;

        public QueueTranslationCommandHandler(IBackgroundTaskQueue backgroundTaskQueue,
            IAggregateRepository<SIOGoogleSynthesizerStoreDbContext> googleSynthesizerAggregateRepository,
            IAggregateRepository<SIOStoreDbContext> storeAggregateRepository,
            IAggregateFactory aggregateFactory)
        {
            if (backgroundTaskQueue is null)
                throw new ArgumentNullException(nameof(backgroundTaskQueue));
            if (googleSynthesizerAggregateRepository is null)
                throw new ArgumentNullException(nameof(googleSynthesizerAggregateRepository));
            if (storeAggregateRepository is null)
                throw new ArgumentNullException(nameof(storeAggregateRepository));
            if (aggregateFactory is null)
                throw new ArgumentNullException(nameof(aggregateFactory));

            _backgroundTaskQueue = backgroundTaskQueue;
            _googleSynthesizerAggregateRepository = googleSynthesizerAggregateRepository;
            _storeAggregateRepository = storeAggregateRepository;
            _aggregateFactory = aggregateFactory;
        }

        public async Task ExecuteAsync(QueueTranslationCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var translation = _aggregateFactory.FromHistory<Translation, TranslationState>(Enumerable.Empty<IEvent>());

                translation.Queue(command.Subject, command.DocumentSubject);

                await _storeAggregateRepository.SaveAsync(translation, command, cancellationToken: cancellationToken);

                await _backgroundTaskQueue.QueueAsync(async (scf, ct) =>
                {
                    using (var scope = scf.CreateScope())
                    {
                        var commandDispatcher = scope.ServiceProvider.GetService<ICommandDispatcher>();
                        await commandDispatcher.DispatchAsync(new StartTranslationCommand(command.Subject, correlationId: CorrelationId.New(), command.Version + 1, Actor.Unknown), ct);
                    }                    

                }, ExecutionType.FireAndForget);
            }
            catch (Exception ex)
            {
                var googleSynthesis = await _googleSynthesizerAggregateRepository.GetAsync<GoogleSynthesize, GoogleSynthesizeState>(command.Subject, cancellationToken);
                googleSynthesis.Fail($"{nameof(QueueTranslationCommandHandler)}.{nameof(QueueTranslationCommandHandler.ExecuteAsync)} - {ex.Message}");
                await _googleSynthesizerAggregateRepository.SaveAsync(googleSynthesis, cancellationToken: cancellationToken);
            }
        }
    }
}
