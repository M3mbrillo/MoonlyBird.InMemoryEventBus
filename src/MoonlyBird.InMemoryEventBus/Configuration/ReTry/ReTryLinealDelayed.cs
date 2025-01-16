using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.Configuration.ReTry;

public class ReTryLinealDelayed(TimeSpan delay) : Abstract.Configuration.IReTryBehavior
{
    public async Task<bool> CanReTryEventAsync<TEvent>(Event<TEvent> @event, CancellationToken cancellationToken) where TEvent : class
    {
        if (@event.Metadata?.RemainingAttempts <= 0)
            return false;
        
        await Task.Delay(delay, cancellationToken);
        return true;
    }
}