using Microsoft.Extensions.DependencyInjection;
using SIO.Domain.Translations.CommandHandlers;
using SIO.Domain.Translations.Commands;
using SIO.Google.Synthesiser.Projections;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddTransient<IInMemoryProjector<TranslationProjection>, TranslationProjector>()
                .AddTransient<ICommandHandler<QueueTranslationCommand>, QueueTranslationCommandHandler>()
                .AddTransient<ICommandHandler<SynthesizeTextCommand>, SynthesizeTextCommandHandler>();

            return services;
        }
    }
}
