using MoonlyBird.InMemoryEventBus.Abstract.ScopeContext;

namespace MoonlyBird.InMemoryEventBus.Abstract;

public interface IConsumer : IAsyncDisposable, IDisposable
{
    ValueTask Start(CancellationToken token = default);
    ValueTask Stop();
    Type GetEventType() => typeof(object);
}

public interface IConsumer<TEvent> : IConsumer
{
    new Type GetEventType() => typeof(TEvent);
    Type IConsumer.GetEventType() => GetEventType();
}