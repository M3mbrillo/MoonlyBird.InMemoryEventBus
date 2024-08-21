namespace MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride;

public interface IDummyExternalService
{
    Guid ScopeIdentifier { get; }
}

public class DummyExternalService : IDummyExternalService
{
    public DummyExternalService(Guid scopeIdentifier)
    {
        ScopeIdentifier = scopeIdentifier;
    }
    
    public Guid ScopeIdentifier { get; init; }
}