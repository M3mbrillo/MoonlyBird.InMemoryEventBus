using MoonlyBird.InMemoryEventBus.Abstract;

namespace HappySnorlax.Snorlax;

public class BigSnorlax : IEventHandler<FeedEvent>
{
    public ValueTask Handle(FeedEvent? dataEvent, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}