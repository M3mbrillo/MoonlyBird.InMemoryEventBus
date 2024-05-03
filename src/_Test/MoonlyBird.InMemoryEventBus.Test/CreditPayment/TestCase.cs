using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;
using MoonlyBird.InMemoryEventBus.Test.CreditPayment.Model;

namespace MoonlyBird.InMemoryEventBus.Test.CreditPayment;

public class TestCase
{
    private Setup _setup = new();
    private IProducer<PaymentEvent> _producer;


    public TestCase()
    {
        
    }

    [Fact]
    async Task Workflow()
    {
        _producer = _setup.GetProducer<PaymentEvent>();
        var consumer = _setup.GetConsumer<PaymentEvent>();
        
        var accountOne = (accountNumber: "XXX", currentAmount: 10_000m);
        var accountTwo = (accountNumber: "YYY", currentAmount: 10_000m);

        Vault.CreateBankAccount(accountOne);
        Vault.CreateBankAccount(accountTwo);
        
        await consumer.Start();

        const decimal suspiciousAmount = 6_000m; 
        
        accountOne.currentAmount = await Pay(accountOne, 100);
        accountTwo.currentAmount = await Pay(accountTwo, 600);
        accountTwo.currentAmount = await Pay(accountTwo, suspiciousAmount); // One suspicius :$
        accountOne.currentAmount = await Pay(accountOne, 500);
        
        Thread.Sleep(TimeSpan.FromSeconds(2));

        Vault.AssertSuspiciousTransaction(suspicius =>
        {
            suspicius = suspicius.ToList();
            
            Assert.Single(suspicius);

            var transaction = suspicius.Single();
            Assert.Equal(accountTwo.accountNumber, transaction.bankAccount);
            Assert.Equal(suspiciousAmount, transaction.amount);
        });
        
        
        Vault.AssertLedger(bankAccounts =>
        {
            if (!bankAccounts.TryGetValue(accountOne.accountNumber, out var credit))
            {
                Assert.Fail("The account one dont exist!");
            }
            Assert.Equal(accountOne.currentAmount, credit);
            
            
            if (!bankAccounts.TryGetValue(accountTwo.accountNumber, out credit))
            {
                Assert.Fail("The account two dont exist!");
            }
            Assert.Equal(accountTwo.currentAmount, credit);
            
        });
        
        await consumer.Stop();
    }

    private async Task<decimal> Pay((string accountNumber, decimal currentAmount) account, decimal paymentAmount)
    {
        var metadata = new EventMetadata(Guid.NewGuid().ToString());
        var @event = new Event<PaymentEvent>(new PaymentEvent(account.accountNumber, paymentAmount), metadata);
        
        await _producer.PublishAsync(@event);
        
        account.currentAmount -= paymentAmount;
        
        return account.currentAmount;
    }
}