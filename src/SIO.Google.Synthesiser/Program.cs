using Microsoft.Extensions.Hosting;
using SIO.Domain.Extensions;
using SIO.Google.Synthesiser.Extensions;
using System.Threading.Tasks;

namespace SIO.Google.Synthesiser
{
    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices((ctx, services) => services.AddInfrastructure(ctx.Configuration)
                    .AddDomain())
                .Build();

            await host.RunAsync();
        }
    }
}