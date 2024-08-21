using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.Abstract;

public interface IProducer : IAsyncDisposable
{
    ValueTask PublishAsync<T>(Event<T> @event, CancellationToken token = default);
}

public interface IProducer<T> : IProducer
{
    ValueTask PublishAsync(Event<T> @event, CancellationToken token = default);

    ValueTask IProducer.PublishAsync<TOther>(Event<TOther> @event, CancellationToken token)
    {
        return PublishAsync(
            @event as Event<T> ?? throw new ArgumentException("Invalid type. Expected type: " + typeof(T).FullName),
            token); 
    }
}