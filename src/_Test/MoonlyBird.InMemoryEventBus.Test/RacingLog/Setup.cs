using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.DependencyInjection;
using MoonlyBird.InMemoryEventBus.ScopeContext;
using MoonlyBird.InMemoryEventBus.Test.RacingLog.EventHandler;
using MoonlyBird.InMemoryEventBus.Test.RacingLog.Model;


namespace MoonlyBird.InMemoryEventBus.Test.RacingLog;

public class Setup
{
    public ServiceProvider Provider { get; set; }
    
    public Setup()
    {
        ServiceCollection serviceCollection = new();
        
        serviceCollection.AddLogging(x => x.AddSimpleConsole());

        serviceCollection
            .AddInMemoryEvent<LongTaskEvent>()
            .AddHandler<SleepingEventHandler>(EnumScopeContext.Message);

        Provider = serviceCollection.BuildServiceProvider();
    }


    public IConsumer<T> GetConsumer<T>()
        => Provider.GetRequiredService<IConsumer<T>>();
    
    public IProducer<T> GetProducer<T>()
        => Provider.GetRequiredService<IProducer<T>>();
}