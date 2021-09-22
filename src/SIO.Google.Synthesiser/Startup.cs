using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SIO.Domain.Documents.Events;
using SIO.Domain.Translations.Events;
using SIO.Google.Credentials;
using SIO.Google.Credentials.Connections;
using SIO.Google.Credentials.Extensions;
using SIO.Google.Synthesiser.Events;
using SIO.Google.Synthesiser.Functions;
using SIO.Google.Synthesiser.Projections;
using SIO.Infrastructure.Azure.ServiceBus.Extensions;
using SIO.Infrastructure.Azure.Storage.Extensions;
using SIO.Infrastructure.EntityFrameworkCore.SqlServer.Extensions;
using SIO.Infrastructure.Extensions;
using SIO.Infrastructure.Serialization.Json.Extensions;
using SIO.Infrastructure.Serialization.MessagePack.Extensions;

[assembly: FunctionsStartup(typeof(SIO.Google.Synthesiser.Startup))]

namespace SIO.Google.Synthesiser
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();

            builder.Services
                .AddTransient<IInMemoryProjector<TranslationProjection>, TranslationProjector>()
                .AddTransient<IEventManager, DefaultEventManager>()
                .AddTransient<IProcessText, ProcessText>()
                .AddSIOInfrastructure()
                    .AddGoogleCredentials(o =>
                    {
                        o.AvaliableCredentials = new GoogleCredentialOptions[]
                        {
                            new GoogleCredentialOptions
                            {
                                Type = config.GetValue<string>("Google__Credentials__type"),
                                ProjectId = config.GetValue<string>("Google__Credentials__project_id"),
                                PrivateKeyId = config.GetValue<string>("Google__Credentials__private_key_id"),
                                PrivateKey = config.GetValue<string>("Google__Credentials__private_key"),
                                ClientEmail = config.GetValue<string>("Google__Credentials__client_email"),
                                ClientId = config.GetValue<string>("Google__Credentials__client_id"),
                                AuthUri = config.GetValue<string>("Google__Credentials__auth_uri"),
                                TokenUri = config.GetValue<string>("Google__Credentials__token_uri"),
                                AuthProviderX509CertUrl = config.GetValue<string>("Google__Credentials__auth_provider_x509_cert_url"),
                                ClientX509CertUrl = config.GetValue<string>("Google__Credentials__client_x509_cert_url")
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
                    .AddAzureStorage(o => o.ConnectionString = config.GetValue<string>("AzureWebJobsStorage"))
                    .AddAzureServiceBus(o => {
                        o.UseConnection(config.GetValue<string>("AzureServiceBus"));
                        o.UseTopic(t =>
                        {
                            t.WithName("generic");
                        });
                    })
                    .AddEntityFrameworkCoreSqlServer(o => {
                        o.AddStore(config.GetConnectionString("Store"));
                    })
                    .AddJsonSerializers();

            if (string.IsNullOrEmpty(config.GetValue<string>("Google__Credentials__type")))
                throw new System.Exception("wtf bro");
        }
    }
}
