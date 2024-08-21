using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.Abstract;

public interface IEventHandler<T>
{
    ValueTask Handle(T? dataEvent, CancellationToken token = default);
    
    ValueTask HandleException(Event<T> dataEvent, Exception exception, CancellationToken token = default)
        => ValueTask.CompletedTask;
    
    ValueTask HandleCancellation(Event<T> dataEvent,CancellationToken token = default)
        => ValueTask.CompletedTask; 
}