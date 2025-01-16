namespace MoonlyBird.InMemoryEventBus.Abstract.Configuration;

public interface IReTryBehavior
{
    Task<bool> CanReTryEventAsync<TEvent>(Model.Event<TEvent> @event, CancellationToken cancellationToken) where TEvent : class;
}