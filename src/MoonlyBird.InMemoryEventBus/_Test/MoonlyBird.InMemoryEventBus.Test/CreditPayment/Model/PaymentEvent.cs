namespace MoonlyBird.InMemoryEventBus.Test.SimpleUse.Model;

public sealed record PaymentEvent(string BankAccound, decimal Amount); 