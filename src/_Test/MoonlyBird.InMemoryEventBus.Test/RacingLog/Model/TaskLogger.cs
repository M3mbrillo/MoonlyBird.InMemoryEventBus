using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace MoonlyBird.InMemoryEventBus.Test.RacingLog.Model;

public static class TaskLogger
{
    private static readonly AsyncLocal<ConcurrentBag<(Guid taskId, LogEvent logEvent, DateTimeOffset eventAt)>> _logger =
        new();
    
    private static ConcurrentBag<(Guid taskId, LogEvent logEvent, DateTimeOffset eventAt)> Logger => _logger.Value ?? throw new NullReferenceException("Call TaskLogger.InitStore in your FACT test to ensure dont shared the same data between test");
    
    public static void InitStore(ConcurrentBag<(Guid taskId, LogEvent logEvent, DateTimeOffset eventAt)> store)
        => _logger.Value = store;
    
    
    public enum LogEvent { Start, End }
    
    
    public static void LogStartTask(LongTaskEvent? dataEvent)
    {
        Logger.Add((dataEvent!.TaskId, LogEvent.Start, DateTimeOffset.Now.UtcDateTime));
    }

    public static void LogEndTask(LongTaskEvent dataEvent)
    {
        Logger.Add((dataEvent!.TaskId, LogEvent.End, DateTimeOffset.Now.UtcDateTime));
    }

    public static IEnumerable<(Guid taskId, LogEvent logEvent, DateTimeOffset eventAt)> GetTracing()
        => Logger.ToImmutableArray();
}