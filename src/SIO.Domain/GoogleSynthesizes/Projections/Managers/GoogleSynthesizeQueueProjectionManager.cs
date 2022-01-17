using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SIO.Domain.GoogleSynthesizes.Events;
using SIO.Infrastructure.EntityFrameworkCore.DbContexts;
using SIO.Infrastructure.Projections;

namespace SIO.Domain.GoogleSynthesizes.Projections.Managers
{
    public sealed class GoogleSynthesizeQueueProjectionManager : ProjectionManager<GoogleSynthesizeQueue>
    {
        private readonly IEnumerable<IProjectionWriter<GoogleSynthesizeQueue>> _projectionWriters;
        private readonly ISIOProjectionDbContextFactory _projectionDbContextFactory;
        private readonly GoogleSynthesizeOptions _GoogleSynthesizeOptions;

        public GoogleSynthesizeQueueProjectionManager(ILogger<ProjectionManager<GoogleSynthesizeQueue>> logger,
            IEnumerable<IProjectionWriter<GoogleSynthesizeQueue>> projectionWriters,
            ISIOProjectionDbContextFactory projectionDbContextFactory,
            IOptionsSnapshot<GoogleSynthesizeOptions> optionsSnapshot) : base(logger)
        {
            if (projectionWriters == null)
                throw new ArgumentNullException(nameof(projectionWriters));
            if (projectionDbContextFactory == null)
                throw new ArgumentNullException(nameof(projectionDbContextFactory));
            if (optionsSnapshot == null)
                throw new ArgumentNullException(nameof(optionsSnapshot));

            _projectionWriters = projectionWriters;
            _projectionDbContextFactory = projectionDbContextFactory;
            _GoogleSynthesizeOptions = optionsSnapshot.Value;

            Handle<GoogleSynthesizeQueued>(HandleAsync);
            Handle<GoogleSynthesizeFailed>(HandleAsync);
            Handle<GoogleSynthesizeSucceded>(HandleAsync);
        }

        public async Task HandleAsync(GoogleSynthesizeQueued @event, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(GoogleSynthesizeQueueProjectionManager)}.{nameof(HandleAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            await Task.WhenAll(_projectionWriters.Select(pw => pw.AddAsync(@event.Subject, () => new GoogleSynthesizeQueue
            {
                Attempts = 0,
                Subject = @event.Subject,
                DocumentSubject = @event.DocumentSubject
            }, cancellationToken)));
        }

        public async Task HandleAsync(GoogleSynthesizeFailed @event, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(GoogleSynthesizeQueueProjectionManager)}.{nameof(HandleAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            using (var context = _projectionDbContextFactory.Create())
            {
                var googleSynthesize = await context.Set<GoogleSynthesizeQueue>().FindAsync(@event.Subject);
                if (googleSynthesize.Attempts == _GoogleSynthesizeOptions.MaxRetries)
                {
                    await Task.WhenAll(_projectionWriters.Select(pw => pw.RemoveAsync(@event.Subject)));
                }
                else
                {
                    await Task.WhenAll(_projectionWriters.Select(pw => pw.UpdateAsync(@event.Subject, epq =>
                    {
                        epq.Attempts++;
                    })));
                }
            }
        }

        public async Task HandleAsync(GoogleSynthesizeSucceded @event, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(GoogleSynthesizeQueueProjectionManager)}.{nameof(HandleAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            await Task.WhenAll(_projectionWriters.Select(pw => pw.RemoveAsync(@event.Subject)));
        }

        public override async Task ResetAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(GoogleSynthesizeQueueProjectionManager)}.{nameof(ResetAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            await Task.WhenAll(_projectionWriters.Select(pw => pw.ResetAsync(cancellationToken)));
        }
    }
}
