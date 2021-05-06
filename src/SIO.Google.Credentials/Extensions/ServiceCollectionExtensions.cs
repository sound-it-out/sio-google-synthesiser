using System;
using Microsoft.Extensions.DependencyInjection;
using SIO.Google.Credentials.Connections;

namespace SIO.Google.Credentials.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGoogleCredentials(this IServiceCollection services, Action<GoogleConnectionOptions> optionsAction)
        {
            services.Configure(optionsAction);
            services.AddSingleton<IGoogleConnectionPool, GoogleConnectionPool>();

            return services;
        }
    }
}
