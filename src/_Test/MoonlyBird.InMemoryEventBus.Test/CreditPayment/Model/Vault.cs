using System.Collections.Concurrent;

namespace MoonlyBird.InMemoryEventBus.Test.CreditPayment.Model;

public static class Vault
{
    private static readonly AsyncLocal<Dictionary<string, decimal>> _bankAccounts = new();
    private static readonly Dictionary<string, decimal> BankAccounts = _bankAccounts.Value ??= new();
    
    private static readonly object _blockConcurrentDebit = new();
    
    public static bool TryDebitAmount(string account, decimal amount, out decimal finalAmount)
    {
        lock (_blockConcurrentDebit)
        {
            if (!BankAccounts.TryGetValue(account, out var currentAmount))
            {
                finalAmount = 0;
                return false;
            }

            if (currentAmount < amount)
            {
                finalAmount = currentAmount;
                return false;
            }

            finalAmount = currentAmount - amount;
            BankAccounts[account] = finalAmount;
            return true;
        }
    }



    private static ConcurrentBag<(string transaction, string bankAccount, decimal amount)> SuspiciousTransactions = new();
    public static void SaveSuspiciousTransaction(PaymentEvent dataEvent, string correlationId)
    {
        SuspiciousTransactions.Add((correlationId, dataEvent.BankAccound, dataEvent.Amount));
    }
    
    public static void AssertSuspiciousTransaction(Action<IEnumerable<(string transaction, string bankAccount, decimal amount)>> fnAsserts)
    {
        fnAsserts(SuspiciousTransactions.ToArray());
    }

    public static void AssertLedger(
        Action<Dictionary<string, decimal>> fnAsserts)
    {
        lock (_blockConcurrentDebit)
        {
            fnAsserts(BankAccounts);
        }
    }

    public static void CreateBankAccount((string accountNumber, decimal currentAmount) account)
    {
        lock (_blockConcurrentDebit)
        {
            BankAccounts.TryAdd(account.accountNumber, account.currentAmount);
        }
    }
}