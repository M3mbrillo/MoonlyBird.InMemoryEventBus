using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;
using MoonlyBird.InMemoryEventBus.Test.CreditPayment.Model;

namespace MoonlyBird.InMemoryEventBus.Test.CreditPayment.EventHandler;

public class LongTransactionEventHandler : IEventHandler<PaymentEvent>
{
    public const string LongTransactionAccount = "__LONG_TRANSACTION_ACCOUNT__";
    
    public async ValueTask Handle(PaymentEvent? dataEvent, CancellationToken token = default)
    {
        if (dataEvent == null) return;
        
        if (dataEvent.BankAccound == LongTransactionAccount)
        {
            Vault.TryDebitAmount(dataEvent.BankAccound, dataEvent.Amount, out _);
            
            // used to force a cancellation via cancellationToken propagation.
            await Task.Delay(-1, token).ConfigureAwait(false);
        }
    }

    public ValueTask HandleCancellation(Event<PaymentEvent> @event, CancellationToken token = default)
    {
        var dataEvent = @event.Data!;
        Vault.TryDebitAmount(dataEvent.BankAccound, -1 * dataEvent.Amount, out _);
        
        return ValueTask.CompletedTask;
    }
}