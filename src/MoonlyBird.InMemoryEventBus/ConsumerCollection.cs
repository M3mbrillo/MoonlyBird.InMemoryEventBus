using System.Collections;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus;

public class ConsumerCollection : IConsumerCollection
{
    private List<IConsumer> _consumers = new();

    public ConsumerCollection(IEnumerable<IConsumer> consumers)
    {
        this._consumers.AddRange(_consumers);
    }

    /// <summary>
    ///     Start all consumers
    /// </summary>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        foreach (var consumer in _consumers)
        {
            await consumer.Start(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task StartAsync<T>(CancellationToken cancellationToken = default)
    {
        var consumer = _consumers.FirstOrDefault(c => c.GetEventType().FullName == typeof(T).FullName);
        ArgumentNullException.ThrowIfNull(consumer, $"Consumer for {typeof(T).FullName} is unregistered");
            
        await consumer.Start(cancellationToken).ConfigureAwait(false);
    }
    
    public IEnumerator<IConsumer> GetEnumerator()
        => _consumers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => this._consumers.Count;
}