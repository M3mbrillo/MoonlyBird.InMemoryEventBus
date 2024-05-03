using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Test.CreditPayment.Model;

namespace MoonlyBird.InMemoryEventBus.Test.CreditPayment.EventHandler;

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