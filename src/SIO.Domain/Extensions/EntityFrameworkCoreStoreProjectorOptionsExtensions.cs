using Microsoft.Extensions.Configuration;
using SIO.Domain.GoogleSynthesizes.Projections;
using SIO.Domain.GoogleSynthesizes.Projections.Managers;
using SIO.EntityFrameworkCore.DbContexts;
using SIO.Infrastructure.EntityFrameworkCore.Extensions;

namespace SIO.Domain.Extensions
{
    public static class EntityFrameworkCoreStoreProjectorOptionsExtensions
    {
        public static void WithDomainProjections(this EntityFrameworkCoreStoreProjectorOptions options, IConfiguration configuration)
            => options.WithProjection<GoogleSynthesizeFailure, GoogleSynthesizeFailureProjectionManager, SIOGoogleSynthesizerStoreDbContext>(o => o.Interval = configuration.GetValue<int>("GoogleSynthesizeFailure:Interval"))
                .WithProjection<GoogleSynthesizeQueue, GoogleSynthesizeQueueProjectionManager, SIOGoogleSynthesizerStoreDbContext>(o => o.Interval = configuration.GetValue<int>("GoogleSynthesizeQueue:Interval"));
    }
}
