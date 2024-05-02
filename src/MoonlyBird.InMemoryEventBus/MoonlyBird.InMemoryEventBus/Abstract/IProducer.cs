using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.Abstract;

public interface IProducer<T> : IAsyncDisposable
{
    ValueTask PublishAsync(Event<T> @event, CancellationToken token = default);
}