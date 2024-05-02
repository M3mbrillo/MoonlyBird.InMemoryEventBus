using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Test.CreditPayment.Model;
using MoonlyBird.InMemoryEventBus.Test.SimpleUse.Model;

namespace MoonlyBird.InMemoryEventBus.Test.CreditPayment.EventHandler;

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