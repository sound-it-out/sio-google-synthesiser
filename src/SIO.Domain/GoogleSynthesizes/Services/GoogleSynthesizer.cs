using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SIO.Domain.GoogleSynthesizes.Projections;
using SIO.Domain.Translations.Commands;
using SIO.Infrastructure;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.EntityFrameworkCore.DbContexts;

namespace SIO.Domain.GoogleSynthesizes.Services
{
    internal sealed class GoogleSynthesizer : BackgroundService
    {
        private readonly IServiceScope _scope;
        private readonly ILogger<GoogleSynthesizer> _logger;
        private readonly IOptionsMonitor<GooglerSynthesizerOptions> _options;
        private readonly ISIOProjectionDbContextFactory _projectionDbContextFactory;
        private readonly string _name;
        private readonly ICommandDispatcher _commandDispatcher;

        public GoogleSynthesizer(IServiceScopeFactory serviceScopeFactory,
            IOptionsMonitor<GooglerSynthesizerOptions> options,
            ILogger<GoogleSynthesizer> logger)
        {
            if (serviceScopeFactory == null)
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _scope = serviceScopeFactory.CreateScope();
            _logger = logger;
            _options = options;
            _projectionDbContextFactory = _scope.ServiceProvider.GetRequiredService<ISIOProjectionDbContextFactory>();
            _commandDispatcher = _scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

            _name = typeof(GoogleSynthesizer).FullName;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(GoogleSynthesizer)}.{nameof(ExecuteAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var context = _projectionDbContextFactory.Create())
                    {
                        var eventsInQueue = await context.Set<GoogleSynthesizeQueue>()
                            .AsQueryable()
                            .AsNoTracking()
                            .Where(gs => gs.Status != GoogleSynthesizeStatus.Processing)
                            .Take(50)
                            .ToArrayAsync(cancellationToken);

                        var correlationId = CorrelationId.New();

                        foreach (var @event in eventsInQueue)
                        {
                            await _commandDispatcher.DispatchAsync(new QueueTranslationCommand(
                                subject: @event.Subject,
                                correlationId: correlationId,
                                version: 0,
                                Actor.Unknown,
                                documentSubject: @event.DocumentSubject
                            ), cancellationToken);
                        }

                        if (eventsInQueue.Count() == 0)
                            await Task.Delay(_options.CurrentValue.Interval, cancellationToken);
                        else
                            await context.SaveChangesAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"Process '{typeof(GoogleSynthesizer).Name}' failed due to an unexpected error. See exception details for more information.");
                    break;
                }
            }
        }
    }
}
