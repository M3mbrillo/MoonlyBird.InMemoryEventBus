using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Extension;
using MoonlyBird.InMemoryEventBus.Test.CreditPayment.EventHandler;
using MoonlyBird.InMemoryEventBus.Test.SimpleUse.Model;

namespace MoonlyBird.InMemoryEventBus.Test.SimpleUse;

public class Setup
{
    public ServiceProvider Provider { get; set; }
    
    public Setup()
    {
        ServiceCollection serviceCollection = new();
        
        serviceCollection.AddLogging(x => x.AddSimpleConsole());

        serviceCollection.AddInMemoryEvent<PaymentEvent, NotifyBigPaymentEventHandler>();
        serviceCollection.AddInMemoryEvent<PaymentEvent, DebitAccountBankEventHandler>();

        Provider = serviceCollection.BuildServiceProvider();
    }


    public IConsumer<T> GetConsumer<T>()
        => Provider.GetRequiredService<IConsumer<T>>();
    
    public IProducer<T> GetProducer<T>()
        => Provider.GetRequiredService<IProducer<T>>();
}