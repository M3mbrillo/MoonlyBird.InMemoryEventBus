namespace MoonlyBird.InMemoryEventBus.Test.CreditPayment.Model;

public sealed record PaymentEvent(string BankAccound, decimal Amount); 