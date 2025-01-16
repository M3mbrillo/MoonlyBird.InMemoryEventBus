using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.DependencyInjection;
using MoonlyBird.InMemoryEventBus.ScopeContext;
using MoonlyBird.InMemoryEventBus.Test.CreditPayment.EventHandler;
using MoonlyBird.InMemoryEventBus.Test.CreditPayment.Model;

namespace MoonlyBird.InMemoryEventBus.Test.CreditPayment;

public class Setup
{
    public ServiceProvider Provider { get; init; }
    
    public Setup()
    {
        ServiceCollection serviceCollection = new();
        
        serviceCollection.AddLogging(x => x.AddSimpleConsole());

        serviceCollection
            .AddInMemoryEvent<PaymentEvent>()
            .AddHandler<NotifyBigPaymentEventHandler>()
            .AddHandler<DebitAccountBankEventHandler>()
            .AddHandler<DetectScammersEventHandler>()
            .AddHandler<LongTransactionEventHandler>()
            ;

        Provider = serviceCollection.BuildServiceProvider();
    }


    public IConsumer<T> GetConsumer<T>()
        => Provider.GetRequiredService<IConsumer<T>>();
    
    public IProducer<T> GetProducer<T>()
        => Provider.GetRequiredService<IProducer<T>>();
}