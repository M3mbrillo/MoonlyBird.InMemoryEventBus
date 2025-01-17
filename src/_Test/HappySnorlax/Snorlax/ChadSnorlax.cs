using HappySnorlax.Utils;
using MoonlyBird.InMemoryEventBus.Abstract;

namespace HappySnorlax.Snorlax;

public class ChadSnorlax : IEventHandler<FeedEvent>
{
    private readonly ILogger<ChadSnorlax> _logger;

    public ChadSnorlax(ILogger<ChadSnorlax> logger)
    {
        _logger = logger;
    }
    
    public async ValueTask Handle(FeedEvent? dataEvent, CancellationToken token = default)
    {
        _logger.LogInformation("ChadSnorlax is a Chad, he try give TO you food <3");

        var killer = new MemoryKiller();
        var random = new RandomThreadSafe();

        var objectSize = random.GetThreadSafeRandomValue(x => x.Next(100, 200));
        var arraySize = random.GetThreadSafeRandomValue(x => x.Next(100, 500));

        
        var bigArrayTask = killer.BigArrays(objectSize, arraySize);
        
        dataEvent!.StuffOfThings = await bigArrayTask;
        
        _logger.LogInformation("ChadSnorlax give to you many bytes :x");
    }
}