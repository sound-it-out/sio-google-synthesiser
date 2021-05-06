using System.Threading;
using SIO.Infrastructure.Events;

namespace SIO.Google.Credentials.Connections
{
    public interface IGoogleConnectionPool
    {
        IGoogleConnection GetConnection(StreamId streamId, CancellationToken cancellationToken = default);
        void ReleaseConnection(StreamId streamId);
    }
}
