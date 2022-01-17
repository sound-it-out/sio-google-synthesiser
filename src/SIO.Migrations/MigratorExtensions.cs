using Microsoft.EntityFrameworkCore;
using SIO.EntityFrameworkCore.DbContexts;
using SIO.Infrastructure.EntityFrameworkCore.DbContexts;
using SIO.Infrastructure.EntityFrameworkCore.Migrations;

namespace SIO.Migrations
{
    public static class MigratorExtensions
    {
        public static Migrator AddContexts(this Migrator migrator)
            => migrator.WithDbContext<SIOProjectionDbContext>(o => o.UseSqlServer("Server=.,1458;Initial Catalog=sio-google-sythensizer-projections;User Id=sa;Password=1qaz-pl,", b => b.MigrationsAssembly("SIO.Migrations")))
                .WithDbContext<SIOGoogleSynthesizerStoreDbContext>(o => o.UseSqlServer("Server=.,1458;Initial Catalog=sio-google-sythensizer-store;User Id=sa;Password=1qaz-pl,", b => b.MigrationsAssembly("SIO.Migrations")));
    }
}
