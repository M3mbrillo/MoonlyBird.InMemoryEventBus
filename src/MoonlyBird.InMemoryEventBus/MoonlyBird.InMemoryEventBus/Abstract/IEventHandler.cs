namespace MoonlyBird.InMemoryEventBus.Abstract;

public interface IEventHandler<in T>
{
    ValueTask Handle(T? dataEvent, CancellationToken token = default);
}