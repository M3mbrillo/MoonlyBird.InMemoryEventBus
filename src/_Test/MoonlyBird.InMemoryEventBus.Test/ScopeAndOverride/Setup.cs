using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.DependencyInjection;
using MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride.EventHandler;
using MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride.Model;

namespace MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride;

public class Setup
{
    public ServiceProvider Provider { get; set; }
    
    public Setup()
    {
        ServiceCollection serviceCollection = new();
        
        serviceCollection.AddLogging(x => x.AddSimpleConsole());
        serviceCollection.AddScoped<IDummyExternalService>(services =>
        {
            var uniqueServiceScope = new DummyExternalService(Guid.NewGuid());
            return uniqueServiceScope;
        });

        serviceCollection
            .AddInMemoryEvent<DummyEvent>()
            .AddConsumerGroupScoped(configureGroup =>
                configureGroup
                    .AddHandler<RewriteModelEventHandler>()
                    .AddHandler<ClearModelEventHandler>()
            );
        
        Provider = serviceCollection.BuildServiceProvider();
    }
    
    public IConsumer<T> GetConsumer<T>()
        => Provider.GetRequiredService<IConsumer<T>>();
    
    public IProducer<T> GetProducer<T>()
        => Provider.GetRequiredService<IProducer<T>>();
}