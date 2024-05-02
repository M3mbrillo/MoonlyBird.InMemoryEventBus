namespace MoonlyBird.InMemoryEventBus.Abstract;

public interface IConsumer : IAsyncDisposable
{
    ValueTask Start(CancellationToken token = default);
    ValueTask Stop(CancellationToken token = default);
    Type GetEventType() => typeof(object);
}

public interface IConsumer<T> : IConsumer
{
    new Type GetEventType() => typeof(T);

    Type IConsumer.GetEventType() => GetEventType();
}