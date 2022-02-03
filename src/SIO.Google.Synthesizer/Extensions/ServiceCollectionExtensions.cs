using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIO.Domain;
using SIO.Domain.Extensions;
using SIO.EntityFrameworkCore.DbContexts;
using SIO.Google.Credentials;
using SIO.Google.Credentials.Extensions;
using SIO.Infrastructure.Azure.Storage.Extensions;
using SIO.Infrastructure.EntityFrameworkCore.DbContexts;
using SIO.Infrastructure.EntityFrameworkCore.Extensions;
using SIO.Infrastructure.EntityFrameworkCore.SqlServer.Extensions;
using SIO.Infrastructure.Extensions;
using SIO.Infrastructure.Serialization.Json.Extensions;
using SIO.Infrastructure.Serialization.MessagePack.Extensions;

namespace SIO.Google.Synthesizer.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSIOInfrastructure()
                .AddEntityFrameworkCoreSqlServer(options =>
                {
                    options.AddStore<SIOStoreDbContext>(configuration.GetConnectionString("Store"), o => o.MigrationsAssembly($"{nameof(SIO)}.{nameof(Migrations)}"));
                    options.AddStore<SIOGoogleSynthesizerStoreDbContext>(configuration.GetConnectionString("GoogleSythensizerStore"), o => o.MigrationsAssembly($"{nameof(SIO)}.{nameof(Migrations)}"));
                    options.AddProjections(configuration.GetConnectionString("Projection"), o => o.MigrationsAssembly($"{nameof(SIO)}.{nameof(Migrations)}"));
                })
                .AddEntityFrameworkCoreStoreProjector(options => options.WithDomainProjections(configuration))
                .AddEvents(o => o.Register(EventHelper.AllEvents))
                .AddCommands()
                .AddBackgroundProcessing(o => o.Capacity = 100)
                .AddJsonSerializers()
                .AddAzureStorage(o => o.ConnectionString = configuration.GetConnectionString("AzureStorage"))
                .AddGoogleCredentials(o =>
                {
                    o.AvaliableCredentials = new GoogleCredentialOptions[]
                    {
                            new GoogleCredentialOptions
                            {
                                Type = configuration.GetValue<string>("Google:Credentials:type"),
                                ProjectId = configuration.GetValue<string>("Google:Credentials:project_id"),
                                PrivateKeyId = configuration.GetValue<string>("Google:Credentials:private_key_id"),
                                PrivateKey = configuration.GetValue<string>("Google:Credentials:private_key"),
                                ClientEmail = configuration.GetValue<string>("Google:Credentials:client_email"),
                                ClientId = configuration.GetValue<string>("Google:Credentials:client_id"),
                                AuthUri = configuration.GetValue<string>("Google:Credentials:auth_uri"),
                                TokenUri = configuration.GetValue<string>("Google:Credentials:token_uri"),
                                AuthProviderX509CertUrl = configuration.GetValue<string>("Google:Credentials:auth_provider_x509_cert_url"),
                                ClientX509CertUrl = configuration.GetValue<string>("Google:Credentials:client_x509_cert_url")
                            }
                    };
                });

            return services;
        }
    }
}
