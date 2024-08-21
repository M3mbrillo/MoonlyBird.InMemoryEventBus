namespace MoonlyBird.InMemoryEventBus.Abstract.Model;

// TODO [DLQ] - Find a best waym maybe Envelop pattern ?
public record Event<T, TEventMetadata>(T? Data, TEventMetadata? Metadata = default)
    where TEventMetadata : EventMetadata;

public record Event<T>(T? Data, EventMetadata? Metadata = default)
    : Event<T, EventMetadata>(Data, Metadata);
