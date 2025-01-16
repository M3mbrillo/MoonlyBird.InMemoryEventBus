using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.Abstract.Configuration;

public interface IDeadLetterQueueBehavior
{
    ValueTask HandleRejectedManyTimesEventAsync<T>(Event<T> @event, CancellationToken cancellationToken);
}