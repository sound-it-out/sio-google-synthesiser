using SIO.Infrastructure.EntityFrameworkCore.Migrations;
using SIO.Migrations;

await new Migrator()
    .AddContexts()
    .RunAsync(args);
