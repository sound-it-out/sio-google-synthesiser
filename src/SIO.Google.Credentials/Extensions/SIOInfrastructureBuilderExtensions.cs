using System;
using Microsoft.Extensions.DependencyInjection;
using SIO.Google.Credentials.Connections;
using SIO.Infrastructure;
using SIO.Infrastructure.Connections.Extensions;

namespace SIO.Google.Credentials.Extensions
{
    public static class SIOInfrastructureBuilderExtensions
    {
        public static ISIOInfrastructureBuilder AddGoogleCredentials(this ISIOInfrastructureBuilder builder, Action<GoogleConnectionOptions> optionsAction)
        {
            builder.Services.Configure(optionsAction);
            builder.AddConnectionPool<GoogleConnection, GoogleConnectionFactory>();

            return builder;
        }
    }
}
