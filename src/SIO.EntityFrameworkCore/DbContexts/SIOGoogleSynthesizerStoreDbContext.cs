using Microsoft.EntityFrameworkCore;
using SIO.Infrastructure.EntityFrameworkCore.DbContexts;

namespace SIO.EntityFrameworkCore.DbContexts
{
    public class SIOGoogleSynthesizerStoreDbContext : SIOStoreDbContextBase<SIOGoogleSynthesizerStoreDbContext>
    {
        public SIOGoogleSynthesizerStoreDbContext(DbContextOptions<SIOGoogleSynthesizerStoreDbContext> options) : base(options)
        {
        }
    }
}
