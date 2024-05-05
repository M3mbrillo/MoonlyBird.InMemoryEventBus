# MoonlyBird.InMemoryEventBus

https://www.nuget.org/packages/MoonlyBird.InMemoryEventBus/

---

Simple InMemoryEventBus inpired in https://github.com/maranmaran/InMemChannelEventBus using Channels of C#

---

## Example

Configure your event and listener handler with `AddInMemoryEvent` method

```csharp
ServiceCollection serviceCollection = new();

serviceCollection.AddLogging(x => x.AddSimpleConsole());

serviceCollection
    .AddInMemoryEvent<PaymentEvent>()
    .WithHandler<NotifyBigPaymentEventHandler>()
    .WithHandler<DebitAccountBankEventHandler>();

var provider = serviceCollection.BuildServiceProvider();

var consumer = provider.GetRequiredService<IConsumer<PaymentEvent>>();

await consumer.Start();
```

### Event
```csharp
public sealed record PaymentEvent(string BankAccound, decimal Amount); 
```

### Handlers

Inherit from `IEventHandler<TEvent>`

`IEventContextAccessor<PaymentEvent> ctx` it has metadada about the event... in a future maybe i will implement a Envelope Wrapper Pattern... maybe..

```csharp
public class NotifyBigPaymentEventHandler : IEventHandler<PaymentEvent>
{
    private readonly IEventContextAccessor<PaymentEvent> _ctx;
    private readonly ILogger<NotifyBigPaymentEventHandler> _logger;

    public NotifyBigPaymentEventHandler(IEventContextAccessor<PaymentEvent> ctx, ILogger<NotifyBigPaymentEventHandler> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }


    public ValueTask Handle(PaymentEvent? dataEvent, CancellationToken token = default)
    {
        if (dataEvent!.Amount >= 5_000)
        {
            _logger.LogWarning("Warning, big payment detected. Account Bank: {accountBank} - Amount: {amount}", dataEvent.BankAccound, dataEvent.Amount);
            Vault.SaveSuspiciousTransaction(dataEvent, _ctx!.Event!.Metadata!.CorrelationId);
        }
        
        return ValueTask.CompletedTask;
    }
}


public class DebitAccountBankEventHandler : IEventHandler<PaymentEvent>
{
    private readonly ILogger<DebitAccountBankEventHandler> _logger;

    public DebitAccountBankEventHandler(ILogger<DebitAccountBankEventHandler> logger)
    {
        _logger = logger;
    }


    public ValueTask Handle(PaymentEvent? dataEvent, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(dataEvent);

        var succes = Vault.TryDebitAmount(dataEvent.BankAccound, dataEvent.Amount, out var finalAmount);

        if (succes)
        {
            _logger.LogInformation("BankAccount: {bankAccount} - Amount: {amount}", dataEvent.BankAccound, finalAmount);
        }
        else
        {
            _logger.LogCritical("Try debit {debitAmount} from: BankAccount {bankAccount} - Amount: {amount}",
                dataEvent.Amount,
                dataEvent.BankAccound,
                finalAmount);
        }
        
        return ValueTask.CompletedTask;
    }
}
```
