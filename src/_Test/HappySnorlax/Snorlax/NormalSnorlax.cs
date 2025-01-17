using MoonlyBird.InMemoryEventBus.Abstract;

namespace HappySnorlax.Snorlax;

public class NormalSnorlax : IEventHandler<FeedEvent>
{
    private readonly ILogger<NormalSnorlax> _logger;

    public NormalSnorlax(ILogger<NormalSnorlax> logger)
    {
        _logger = logger;
    }
    
    public ValueTask Handle(FeedEvent? dataEvent, CancellationToken token = default)
    {
        _logger.LogInformation("NormalSnorlax do nothings, only eat and sleep");
        return ValueTask.CompletedTask;
    }
}