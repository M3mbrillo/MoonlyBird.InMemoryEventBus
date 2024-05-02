using System.Threading.Channels;
using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus;

public sealed class InMemoryEventBusProducer<T> : Abstract.IProducer<T>
{
    private readonly ChannelWriter<Event<T>> _bus;

    public InMemoryEventBusProducer(ChannelWriter<Event<T>> bus)
    {
        _bus = bus;
    }
    
    public async ValueTask PublishAsync(Event<T> @event, CancellationToken token = default)
    {
        await _bus.WriteAsync(@event, token).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        _bus.TryComplete();
        return ValueTask.CompletedTask;
    }
}