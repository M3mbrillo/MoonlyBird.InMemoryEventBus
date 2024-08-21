namespace MoonlyBird.InMemoryEventBus.Abstract.Model;

// TODO [DLQ] - Find a best waym maybe Envelop pattern ?
public record EventMetadata(string CorrelationId, uint RemainingAttempts = 0);