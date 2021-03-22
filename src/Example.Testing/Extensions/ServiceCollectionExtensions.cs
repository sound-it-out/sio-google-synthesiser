using System;
using Example.Testing.Stubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenEventSourcing.EntityFrameworkCore.InMemory;
using OpenEventSourcing.Events;
using OpenEventSourcing.Extensions;

namespace Example.Testing.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryDatabase<TDbContext>(this IServiceCollection source) where TDbContext : DbContext
        {
            source.AddDbContext<TDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            source.AddOpenEventSourcing()
                .AddEntityFrameworkCoreInMemory();
            return source;
        }

        public static IServiceCollection AddInMemoryEventBus(this IServiceCollection source)
        {
            source.AddScoped<InMemoryEventBus>();
            source.AddScoped<IEventBusPublisher, InMemoryEventBus>(sp => sp.GetRequiredService<InMemoryEventBus>());
            source.AddScoped<IEventBusConsumer, InMemoryEventBus>(sp => sp.GetRequiredService<InMemoryEventBus>());
            return source;
        }
    }
}
