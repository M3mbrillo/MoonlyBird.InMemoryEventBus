using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.DependencyInjection;

public sealed class ConfigureEventBus<TEvent>
{
    private readonly IServiceCollection _serviceCollection;
    
    public ConfigureEventBus(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
        
        // TODO: Expose configuration options and allow user to customize
        _serviceCollection.TryAddSingleton<Channel<Event<TEvent>>>(
            service => Channel.CreateUnbounded<Event<TEvent>>(new UnboundedChannelOptions()
            {
                AllowSynchronousContinuations = false
            }));
        
        // Producer for the event channel
        _serviceCollection.TryAddSingleton<IProducer<TEvent>>(
            service =>
            {
                var channel = service.GetRequiredService<Channel<Event<TEvent>>>();

                return new InMemoryEventBusProducer<TEvent>(channel.Writer);
            }
        );
        
        // Consumer for the event channel
        _serviceCollection.TryAddSingleton(
            typeof(IConsumer<TEvent>),
            service =>
            {
                var channel = service.GetRequiredService<Channel<Event<TEvent>>>();
                var serviceScopeFactory = service.GetRequiredService<IServiceScopeFactory>();
                var logger = service.GetRequiredService<ILoggerFactory>()
                    .CreateLogger<InMemoryEventBusConsumer<TEvent>>();

                return new InMemoryEventBusConsumer<TEvent>(
                    channel.Reader, serviceScopeFactory, logger
                );
            }
        );

        // Register context accessor...
        _serviceCollection.TryAddSingleton(
            typeof(IEventContextAccessor<TEvent>),
            typeof(EventContextAccessor<TEvent>));
    }
    
    public ConfigureEventBus<TEvent> WithHandler<THandler>()
        where THandler : class, IEventHandler<TEvent>
    {
        // Add the handler...
        _serviceCollection.AddScoped<IEventHandler<TEvent>, THandler>();
        
        return this;
    }
}