using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Test.RacingLog.Model;

namespace MoonlyBird.InMemoryEventBus.Test.RacingLog.EventHandler;

public class SleepingEventHandler : IEventHandler<Model.LongTaskEvent>
{
    public async ValueTask Handle(LongTaskEvent? dataEvent, CancellationToken token = default)
    {
        TaskLogger.LogStartTask(dataEvent);
        
        await Task.Delay(dataEvent!.SleepTime, token);
        
        TaskLogger.LogEndTask(dataEvent);
    }
}