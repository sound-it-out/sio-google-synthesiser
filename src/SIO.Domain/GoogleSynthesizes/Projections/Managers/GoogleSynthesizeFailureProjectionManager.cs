using Microsoft.Extensions.Logging;
using SIO.Domain.GoogleSynthesizes.Events;
using SIO.Infrastructure;
using SIO.Infrastructure.EntityFrameworkCore.DbContexts;
using SIO.Infrastructure.Projections;

namespace SIO.Domain.GoogleSynthesizes.Projections.Managers
{
    public sealed class GoogleSynthesizeFailureProjectionManager : ProjectionManager<GoogleSynthesizeFailure>
    {
        private readonly IEnumerable<IProjectionWriter<GoogleSynthesizeFailure>> _projectionWriters;

        public GoogleSynthesizeFailureProjectionManager(ILogger<ProjectionManager<GoogleSynthesizeFailure>> logger,
            IEnumerable<IProjectionWriter<GoogleSynthesizeFailure>> projectionWriters) : base(logger)
        {
            if (projectionWriters == null)
                throw new ArgumentNullException(nameof(projectionWriters));

            _projectionWriters = projectionWriters;

            Handle<GoogleSynthesizeFailed>(HandleAsync);
        }

        public async Task HandleAsync(GoogleSynthesizeFailed @event, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(GoogleSynthesizeQueueProjectionManager)}.{nameof(HandleAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            await Task.WhenAll(_projectionWriters.Select(pw => pw.AddAsync(@event.Subject, () => new GoogleSynthesizeFailure
            {
                Subject = Subject.New(),
                Error = @event.Error,
                DocumentSubject = @event.DocumentSubject
            }, cancellationToken)));
        }

        public override async Task ResetAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(GoogleSynthesizeFailureProjectionManager)}.{nameof(ResetAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            await Task.WhenAll(_projectionWriters.Select(pw => pw.ResetAsync(cancellationToken)));
        }
    }
}
