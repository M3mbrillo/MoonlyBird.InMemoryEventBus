namespace MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride.Model;

public sealed class DummyEvent
{
    public static string DefaultValue => "*-DummyEvent-*";

    public string AnyData { get; set; } = DefaultValue;
    
    public TimeSpan DelayRewriteHandler { get; init; } = TimeSpan.Zero;
    public TimeSpan DelayClearHandler { get; init; } = TimeSpan.Zero;
}