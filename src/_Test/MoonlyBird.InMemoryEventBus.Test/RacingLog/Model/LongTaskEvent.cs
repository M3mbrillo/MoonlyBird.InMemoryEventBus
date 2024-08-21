namespace MoonlyBird.InMemoryEventBus.Test.RacingLog.Model;


public sealed record LongTaskEvent(Guid TaskId, TimeSpan SleepTime);