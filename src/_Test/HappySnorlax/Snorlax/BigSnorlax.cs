using HappySnorlax.Utils;
using MoonlyBird.InMemoryEventBus.Abstract;

namespace HappySnorlax.Snorlax;

public class BigSnorlax : IEventHandler<FeedEvent>
{
    private readonly ILogger<BigSnorlax> _logger;

    public BigSnorlax(ILogger<BigSnorlax> logger)
    {
        _logger = logger;
    }
    
    private static readonly RandomThreadSafe Random = new();
    public byte[][] BigArrayResult { get; set; }
    
    public async ValueTask Handle(FeedEvent? dataEvent, CancellationToken token = default)
    {
        _logger.LogInformation("BigSnorlax is eating...");
        
        var killer = new MemoryKiller();
        var bigArrayTask = killer.BigArrays(
            Random.GetThreadSafeRandomValue(x => x.Next(100, 200)), 
            Random.GetThreadSafeRandomValue(x => x.Next(100, 500))
            );
        
        /*otras formas que quiera testear ...*/
        
        BigArrayResult = await bigArrayTask;
        
        _logger.LogInformation("BigSnorlax eat many bytes as BigArray :x");
    }
}