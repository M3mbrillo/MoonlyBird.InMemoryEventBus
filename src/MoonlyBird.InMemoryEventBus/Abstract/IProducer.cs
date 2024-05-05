using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.Abstract;

public interface IProducer : IAsyncDisposable
{
    ValueTask PublishAsync(Event<object> @event, CancellationToken token = default);
}

public interface IProducer<T> : IProducer
{
    ValueTask PublishAsync(Event<T> @event, CancellationToken token = default);

    ValueTask IProducer.PublishAsync(Event<object> @event, CancellationToken token)
        => PublishAsync((@event as Event<T>) ?? throw new ArgumentException("Invalid type. Expected type: " + typeof(Event<T>).FullName), token);
}