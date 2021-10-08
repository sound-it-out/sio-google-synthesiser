using System.Threading.Tasks;
using SIO.Infrastructure.Events;
using SIO.Infrastructure.Projections;

namespace SIO.Google.Synthesiser.Projections
{
    public interface IInMemoryProjector<TProjection>
        where TProjection : IProjection
    {
        Task<TProjection> ProjectAsync(StreamId streamId, params IEvent[] events);
    }
}
