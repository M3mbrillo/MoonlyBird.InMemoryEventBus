using HappySnorlax.Utils;
using MoonlyBird.InMemoryEventBus.Abstract;

namespace HappySnorlax.Snorlax;

public class LazySnorlax : IEventHandler<FeedEvent>
{
    private readonly ILogger<LazySnorlax> _logger;

    public LazySnorlax(ILogger<LazySnorlax> logger)
    {
        _logger = logger;
    }
    
    public async ValueTask Handle(FeedEvent? dataEvent, CancellationToken token = default)
    {
        var random = new RandomThreadSafe();
        var sleepSec = random.GetThreadSafeRandomValue(r => r.Next(1, 7));

        _logger.LogInformation($"LazySnorlax are sleeping for {sleepSec} seconds");
        await Task.Delay(TimeSpan.FromSeconds(sleepSec), token);
    }
}