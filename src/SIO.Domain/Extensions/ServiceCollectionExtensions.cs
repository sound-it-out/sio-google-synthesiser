using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using SIO.Domain.GoogleSynthesizes;
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
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

            services.AddScoped<ICommandHandler<QueueGoogleSynthesizeCommand>, QueueGoogleSynthesizeCommandHandler>();
            services.AddScoped<ICommandHandler<QueueTranslationCommand>, QueueTranslationCommandHandler>();
            services.AddScoped<ICommandHandler<StartTranslationCommand>, StartTranslationCommandHandler>();

            services.AddHostedService<EventProcessor>();
            services.AddHostedService<GooglerSynthesizer>();

            services.Configure<EventProcessorOptions>(o => o.Interval = 300);
            services.Configure<GooglerSynthesizerOptions>(o => o.Interval = 300);
            services.Configure<GoogleSynthesizeOptions>(o => o.MaxRetries = 5);
            return services;
        }
    }
}
