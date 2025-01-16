using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace HappySnorlax.Snorlax;

public class AngrySnorlax : IEventHandler<FeedEvent>
{
    private readonly ILogger<AngrySnorlax> _logger;

    public AngrySnorlax(ILogger<AngrySnorlax> logger)
    {
        _logger = logger;
    }
    
    public ValueTask Handle(FeedEvent? dataEvent, CancellationToken token = default)
    {
        _logger.LogWarning("I am angry >_<");

        throw new AngryFeelException();
    }

    public ValueTask HandleException(Event<FeedEvent> dataEvent, Exception exception, CancellationToken token = default)
    {
        if (exception is AngryFeelException ||
            exception.InnerException is AngryFeelException
            )
            _logger.LogWarning("Shh shh dont worry, dont worry *hug*");
        else
            _logger.LogCritical("I am angry and dont know why :( help me");
        
        return ValueTask.CompletedTask;
    }
}

public class AngryFeelException : Exception
{
    public AngryFeelException() : base("I am so angry :(")
    {
        
    }
}