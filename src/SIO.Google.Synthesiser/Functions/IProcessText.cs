using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SIO.Google.Synthesiser.Functions
{
    internal interface IProcessText
    {
        Task ExecuteAsync(ProcessTextRequest request, IDurableOrchestrationClient client, CancellationToken cancellationToken = default);
    }
}
