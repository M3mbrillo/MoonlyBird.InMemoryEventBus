using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;
using MoonlyBird.InMemoryEventBus.Test.CreditPayment.Model;

namespace MoonlyBird.InMemoryEventBus.Test.CreditPayment.EventHandler;

public class DetectScammersEventHandler : IEventHandler<PaymentEvent>
{
    public const string ScammerAccount = "__SCAMMER_ACCOUNT__";

    public ValueTask Handle(PaymentEvent? dataEvent, CancellationToken token = default)
    {
        if (dataEvent is null) return ValueTask.CompletedTask;
        
        if (dataEvent.BankAccound == ScammerAccount)
            throw new Exception("Account scammer detected!");
        
        return ValueTask.CompletedTask;
    }

    public ValueTask HandleException(Event<PaymentEvent> dataEvent, Exception exception, CancellationToken token = default)
    {
        Vault.SaveSuspiciousTransaction(dataEvent.Data!, dataEvent.Metadata!.CorrelationId);

        return ValueTask.CompletedTask;
    }
}