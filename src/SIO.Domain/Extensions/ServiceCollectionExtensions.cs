using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIO.Domain.GoogleSynthesizes.CommandHandlers;
using SIO.Domain.GoogleSynthesizes.Commands;
using SIO.Domain.GoogleSynthesizes.Services;
using SIO.Domain.Translations.CommandHandlers;
using SIO.Domain.Translations.Commands;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

            services.AddScoped<ICommandHandler<QueueGoogleSynthesizeCommand>, QueueGoogleSynthesizeCommandHandler>();
            services.AddScoped<ICommandHandler<QueueTranslationCommand>, QueueTranslationCommandHandler>();
            services.AddScoped<ICommandHandler<StartTranslationCommand>, StartTranslationCommandHandler>();

            services.AddHostedService<EventProcessor>();
            services.AddHostedService<GoogleSynthesizer>();

            services.Configure<EventProcessorOptions>(configuration.GetSection(nameof(EventProcessor)));
            services.Configure<GooglerSynthesizerOptions>(configuration.GetSection(nameof(GoogleSynthesizer)));
            return services;
        }
    }
}
