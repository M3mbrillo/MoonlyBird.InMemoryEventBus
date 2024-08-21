using System.Collections.Concurrent;

namespace MoonlyBird.InMemoryEventBus.Test.CreditPayment.Model;

public static class Vault
{
    
    private static readonly AsyncLocal<ConcurrentBag<(string transaction, string bankAccount, decimal amount)>> _suspiciousTransactions = new();
    private static ConcurrentBag<(string transaction, string bankAccount, decimal amount)> SuspiciousTransactions => _suspiciousTransactions.Value ?? throw new NullReferenceException("Call Vault.InitVaultStores in your FACT test to ensure dont shared the same data between test");
    
    private static readonly AsyncLocal<Dictionary<string, decimal>> _bankAccounts = new();
    private static Dictionary<string, decimal> BankAccounts => _bankAccounts.Value ?? throw new NullReferenceException("Call Vault.InitVaultStores in your FACT test to ensure dont shared the same data between test");
    
    
    private static readonly object _lockerConcurrentDebit = new();

    
    public static void InitVaultStores(
        in Dictionary<string, decimal> bankAccountSore,
        in ConcurrentBag<(string transaction, string bankAccount, decimal amount)> suspiciousTransactions)
    {
        _bankAccounts.Value = bankAccountSore;
        _suspiciousTransactions.Value = suspiciousTransactions;
    }
    
    public static bool TryDebitAmount(string account, decimal amount, out decimal finalAmount)
    {
        lock (_lockerConcurrentDebit)
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
        lock (_lockerConcurrentDebit)
        {
            fnAsserts(BankAccounts);
        }
    }

    public static void CreateBankAccount((string accountNumber, decimal currentAmount) account)
    {
        lock (_lockerConcurrentDebit)
        {
            BankAccounts.TryAdd(account.accountNumber, account.currentAmount);
        }
    }
}