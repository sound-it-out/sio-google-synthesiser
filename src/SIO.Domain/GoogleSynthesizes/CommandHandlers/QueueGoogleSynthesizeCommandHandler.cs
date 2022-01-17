using Microsoft.Extensions.Logging;
using SIO.Domain.GoogleSynthesizes.Aggregates;
using SIO.Domain.GoogleSynthesizes.Commands;
using SIO.EntityFrameworkCore.DbContexts;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Domain;
using SIO.Infrastructure.Events;

namespace SIO.Domain.GoogleSynthesizes.CommandHandlers
{
    internal sealed class QueueGoogleSynthesizeCommandHandler : ICommandHandler<QueueGoogleSynthesizeCommand>
    {
        private readonly ILogger<QueueGoogleSynthesizeCommandHandler> _logger;
        private readonly IAggregateRepository<SIOGoogleSynthesizerStoreDbContext> _aggregateRepository;
        private readonly IAggregateFactory _aggregateFactory;

        public QueueGoogleSynthesizeCommandHandler(ILogger<QueueGoogleSynthesizeCommandHandler> logger,
            IAggregateRepository<SIOGoogleSynthesizerStoreDbContext> aggregateRepository,
            IAggregateFactory aggregateFactory)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (aggregateRepository == null)
                throw new ArgumentNullException(nameof(aggregateRepository));
            if (aggregateFactory == null)
                throw new ArgumentNullException(nameof(aggregateFactory));

            _logger = logger;
            _aggregateRepository = aggregateRepository;
            _aggregateFactory = aggregateFactory;
        }

        public async Task ExecuteAsync(QueueGoogleSynthesizeCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(QueueGoogleSynthesizeCommandHandler)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            var aggregate = await _aggregateRepository.GetAsync<GoogleSynthesize, GoogleSynthesizeState>(command.Subject, cancellationToken);

            if (aggregate != null)
                return;

            aggregate = _aggregateFactory.FromHistory<GoogleSynthesize, GoogleSynthesizeState>(Enumerable.Empty<IEvent>());

            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            aggregate.Queue(
                subject: command.Subject,
                publicationDate: command.PublicationDate,
                eventSubject: command.EventSubject
            );

            await _aggregateRepository.SaveAsync(aggregate, command, cancellationToken: cancellationToken);
        }
    }
}
