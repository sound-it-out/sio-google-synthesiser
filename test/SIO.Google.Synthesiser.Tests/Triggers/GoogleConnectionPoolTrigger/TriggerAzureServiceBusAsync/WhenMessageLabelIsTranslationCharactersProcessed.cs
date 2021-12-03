using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SIO.Domain.Documents.Events;
using SIO.Domain.Translations.Events;
using SIO.Google.Credentials.Connections;
using SIO.Google.Synthesiser.Projections;
using SIO.Infrastructure;
using SIO.Infrastructure.Azure.ServiceBus.Messages;
using SIO.Infrastructure.Connections.Pooling;
using SIO.Infrastructure.Events;
using SIO.Infrastructure.Projections;
using SIO.Infrastructure.Testing.Abstractions;
using SIO.Infrastructure.Testing.Attributes;
using Xunit.Abstractions;

namespace SIO.Google.Synthesiser.Tests.Triggers.GoogleConnectionPoolTrigger.TriggerAzureServiceBusAsync
{
    public sealed class WhenMessageLabelIsTranslationCharactersProcessed : Specification
    {
        private readonly Mock<IEventContextFactory> _mockEventContextFactory = new();
        private readonly Mock<IInMemoryProjector<TranslationProjection>> _mockProjector = new();
        private readonly Mock<IConnectionPool<GoogleConnection>> _mockConnectionPool = new();
        private readonly StreamId _streamId = StreamId.New();
        private readonly Message _message = new();

        public WhenMessageLabelIsTranslationCharactersProcessed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected override void BuildServices(IServiceCollection services)
        {
            services.AddSingleton(_mockEventContextFactory.Object);
            services.AddSingleton(_mockProjector.Object);
            services.AddSingleton(_mockConnectionPool.Object);
            services.AddScoped<SIO.Google.Synthesiser.Triggers.GoogleConnectionPoolTrigger>();
        }

        protected sealed override async Task Given()
        {
            var trigger = _serviceProvider.GetRequiredService<SIO.Google.Synthesiser.Triggers.GoogleConnectionPoolTrigger>();
            await trigger.TriggerAzureServiceBusAsync(_message);
        }

        protected sealed override Task When() 
        {
            _mockEventContextFactory.Setup(ecf => ecf.CreateContext(_message)).Returns(new EventContext<IEvent>(_streamId, new TranslationCharactersProcessed("", 0, "", 0), null, null, DateTimeOffset.UtcNow, Actor.Unknown, null));
            _mockProjector.Setup(p => p.ProjectAsync(_streamId)).ReturnsAsync(new TranslationProjection(new Mock<ILogger<ProjectionManager<TranslationProjection>>>().Object)
            {
                TranslationType = TranslationType.Google
            });

            _message.Label = nameof(TranslationCharactersProcessed);

            return Task.CompletedTask;
        }

        [Then]
        public void ShouldReleaseConnection()
        {
            _mockConnectionPool.Verify(cp => cp.ReleaseConnection(_streamId));
        }
    }
}
