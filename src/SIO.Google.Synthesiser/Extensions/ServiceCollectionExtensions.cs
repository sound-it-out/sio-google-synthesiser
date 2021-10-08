using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIO.Domain.Documents.Events;
using SIO.Domain.Translations.Events;
using SIO.Google.Credentials;
using SIO.Google.Credentials.Extensions;
using SIO.Infrastructure.Azure.ServiceBus.Extensions;
using SIO.Infrastructure.Azure.Storage.Extensions;
using SIO.Infrastructure.EntityFrameworkCore.SqlServer.Extensions;
using SIO.Infrastructure.Extensions;
using SIO.Infrastructure.Serialization.Json.Extensions;
using SIO.Infrastructure.Serialization.MessagePack.Extensions;

namespace SIO.Google.Synthesiser.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSIOInfrastructure()
                    .AddEntityFrameworkCoreSqlServer(options => {
                        options.AddStore(configuration.GetConnectionString("Store"));
                        options.AddProjections(configuration.GetConnectionString("Projection"));
                    })
                    .AddGoogleCredentials(o =>
                    {
                        o.AvaliableCredentials = new GoogleCredentialOptions[]
                        {
                            new GoogleCredentialOptions
                            {
                                Type = configuration.GetValue<string>("Google__Credentials__type"),
                                ProjectId = configuration.GetValue<string>("Google__Credentials__project_id"),
                                PrivateKeyId = configuration.GetValue<string>("Google__Credentials__private_key_id"),
                                PrivateKey = configuration.GetValue<string>("Google__Credentials__private_key"),
                                ClientEmail = configuration.GetValue<string>("Google__Credentials__client_email"),
                                ClientId = configuration.GetValue<string>("Google__Credentials__client_id"),
                                AuthUri = configuration.GetValue<string>("Google__Credentials__auth_uri"),
                                TokenUri = configuration.GetValue<string>("Google__Credentials__token_uri"),
                                AuthProviderX509CertUrl = configuration.GetValue<string>("Google__Credentials__auth_provider_x509_cert_url"),
                                ClientX509CertUrl = configuration.GetValue<string>("Google__Credentials__client_x509_cert_url")
                            }
                        };
                    })
                    .AddEvents(o =>
                    {
                        o.Register<DocumentUploaded>();
                        o.Register<TranslationQueued>();
                        o.Register<TranslationStarted>();
                        o.Register<TranslationCharactersProcessed>();
                        o.Register<TranslationFailed>();
                    })
                    .AddCommands()
                    .AddAzureStorage(o => o.ConnectionString = configuration.GetValue<string>("AzureWebJobsStorage"))
                    .AddAzureServiceBus(o => {
                        o.UseConnection(configuration.GetValue<string>("AzureServiceBus"));
                        o.UseTopic(t =>
                        {
                            t.WithName("generic");
                        });
                    })
                    .AddEntityFrameworkCoreSqlServer(o => {
                        o.AddStore(configuration.GetConnectionString("Store"));
                    })
                    .AddJsonSerializers();

            return services;
        }
    }
}
