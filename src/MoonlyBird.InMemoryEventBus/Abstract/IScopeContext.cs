using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.Abstract;

public interface IScopeContext<TEvent>
{
    void DeferredHandlersExecution(Event<TEvent> @event, CancellationToken cancellationToken);
}