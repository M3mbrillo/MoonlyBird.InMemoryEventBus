using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;
using MoonlyBird.InMemoryEventBus.Abstract.ScopeContext;
using MoonlyBird.InMemoryEventBus.ScopeContext;

namespace MoonlyBird.InMemoryEventBus.DependencyInjection;

public sealed class ConfigureEventBus<TEvent>
{
    private readonly IServiceCollection _serviceCollection;
    
    public ConfigureEventBus(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
        
        // Create Channel to shared event between thread
        _serviceCollection.TryAddSingleton<Channel<Event<TEvent>>>(
            service => Channel.CreateUnbounded<Event<TEvent>>(new UnboundedChannelOptions()
            {
                AllowSynchronousContinuations = false
            }));
        
        // Create a Producer (a channel writer) to publish an event
        _serviceCollection.TryAddSingleton<IProducer<TEvent>>(
            service =>
            {
                var channel = service.GetRequiredService<Channel<Event<TEvent>>>();

                return new InMemoryEventBusProducer<TEvent>(channel.Writer);
            }
        );
        
        // Create a Consumer (a channel reader) to read each event, resolve handle creation and execution
        _serviceCollection.TryAddSingleton(
            typeof(IConsumer<TEvent>),
            service =>
            {
                var channel = service.GetRequiredService<Channel<Event<TEvent>>>();
                var serviceScopeFactory = service.GetRequiredService<IServiceScopeFactory>();
                var logger = service.GetRequiredService<ILoggerFactory>()
                    .CreateLogger<InMemoryEventBusConsumer<TEvent>>();
                
                
                return new InMemoryEventBusConsumer<TEvent>(
                    channel.Reader,
                    serviceScopeFactory,
                    logger
                );
            }
        );
        
        _serviceCollection.AddSingleton<IScopeContextMessage<TEvent>, ScopeContextMessage<TEvent>>();

        // Register context accessor...
        _serviceCollection.TryAddSingleton(typeof(IEventContextAccessor<TEvent>), typeof(EventContextAccessor<TEvent>));
    }
    
    // [Obsolete("Use scopeContexts instead")]
    // public ConfigureEventBus<TEvent> WithHandler<THandler>()
    //     where THandler : class, IEventHandler<TEvent>
    // {
    //     _serviceCollection.AddScoped<IEventHandler<TEvent>, THandler>();
    //     
    //     return this;
    // }

    public ConfigureEventBus<TEvent> AddHandler<THandler>(EnumScopeContext scopeContext = EnumScopeContext.Message)
        where THandler : class, IEventHandler<TEvent>
    {
        var key = BaseScopeContext<TEvent>.GenerateServiceHandlerKey(scopeContext);
        
        // Yes, I can duplicate a key.
        // See https://github.com/dotnet/runtime/blob/release/8.0/src/libraries/Microsoft.Extensions.DependencyInjection.Specification.Tests/src/KeyedDependencyInjectionSpecificationTests.cs 
        _serviceCollection.AddKeyedScoped<IEventHandler<TEvent>, THandler>(key);
        return this;
    }
    
}