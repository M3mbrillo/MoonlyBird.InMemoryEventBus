using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.Abstract;

public interface IEventContextAccessor<T>
{
    public Event<T>? Event { get; }
    void Set(Event<T> @event);
}