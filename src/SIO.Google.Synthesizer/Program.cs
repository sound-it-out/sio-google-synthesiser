using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SIO.Domain.Extensions;
using SIO.EntityFrameworkCore.DbContexts;
using SIO.Google.Synthesizer.Extensions;
using SIO.Infrastructure.EntityFrameworkCore.Extensions;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddInfrastructure(hostContext.Configuration)
            .AddDomain();
    })
    .Build();

var env = host.Services.GetRequiredService<IHostEnvironment>();

if (env.IsDevelopment())
{
    await host.RunProjectionMigrationsAsync();
    await host.RunStoreMigrationsAsync<SIOGoogleSynthesizerStoreDbContext>();
}

await host.RunAsync();
