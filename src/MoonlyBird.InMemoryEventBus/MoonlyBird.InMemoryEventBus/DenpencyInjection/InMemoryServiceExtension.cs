using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.Extension;

public static class InMemoryServiceExtension
{
    public static IServiceCollection AddInMemoryEvent<T, THandler>(this IServiceCollection services)
        where THandler : class, IEventHandler<T>
    {
        // TODO: Expose configuration options and allow user to customize
        var bus = Channel.CreateUnbounded<Event<T>>(
            new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = false,
            }
        );

        // typed event handler
        services.AddScoped<IEventHandler<T>, THandler>();

        // typed event producer
        services.AddSingleton(typeof(IProducer<T>), _ => new InMemoryEventBusProducer<T>(bus.Writer));

        // typed event consumer
        var consumerFactory = (IServiceProvider provider) => new InMemoryEventBusConsumer<T>(
            bus.Reader,
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILoggerFactory>().CreateLogger<InMemoryEventBusConsumer<T>>()
        );
        services.AddSingleton(typeof(IConsumer), consumerFactory.Invoke);
        services.AddSingleton(typeof(IConsumer<T>), consumerFactory.Invoke);

        // typed event context accessor
        services.AddSingleton(typeof(IEventContextAccessor<T>), typeof(EventContextAccessor<T>));

        return services;
    }

}