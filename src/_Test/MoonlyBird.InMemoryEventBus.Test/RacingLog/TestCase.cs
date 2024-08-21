using System.Collections.Immutable;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;
using MoonlyBird.InMemoryEventBus.Test.RacingLog.Model;

namespace MoonlyBird.InMemoryEventBus.Test.RacingLog;

public class TestCase
{
    private Setup _setup = new();
    private IProducer<LongTaskEvent> _producer => _setup.GetProducer<LongTaskEvent>();

    
    [Fact]
    async Task RacingOverlap()
    {
        CancellationToken cancellationToken = CancellationToken.None;
        
        TaskLogger.InitStore(new ());
        IConsumer<LongTaskEvent> consumer = _setup.GetConsumer<LongTaskEvent>();
        await consumer.Start(cancellationToken);
        
        
        LongTaskEvent p1 = new LongTaskEvent(new Guid("11111111-0000-0000-0000-000000000000"), TimeSpan.FromSeconds(5));
        await DeleyedPublishEvent(p1, cancellationToken);
        // start t0 - end t5 -- 2nd place
        
        LongTaskEvent p2 = new LongTaskEvent(new Guid("22222222-0000-0000-0000-000000000000"), TimeSpan.FromSeconds(15));
        await DeleyedPublishEvent(p2, cancellationToken);
        // start t1 - end t16 -- 6th place
        
        LongTaskEvent p3 = new LongTaskEvent(new Guid("33333333-0000-0000-0000-000000000000"), TimeSpan.FromSeconds(2));
        await DeleyedPublishEvent(p3, cancellationToken);
        // start t2 - end t4 -- 1st place
        
        /*-----------------------------------------*/
        await Task.Delay(TimeSpan.FromSeconds(6) , cancellationToken);
        // t3 -- Resume at t9
        /*-----------------------------------------*/
        
        LongTaskEvent p4 = new LongTaskEvent(new Guid("44444444-0000-0000-0000-000000000000"), TimeSpan.FromSeconds(3));
        await DeleyedPublishEvent(p4, cancellationToken);
        // start t9 - end t12 -- 3rd place
        
        LongTaskEvent p5 = new LongTaskEvent(new Guid("55555555-0000-0000-0000-000000000000"), TimeSpan.FromSeconds(4));
        await DeleyedPublishEvent(p5, cancellationToken);
        // start t10 - end t14 -- 4th place
        
        await Task.Delay(TimeSpan.FromSeconds(6) , cancellationToken);
        
        var store = TaskLogger.GetTracing().ToArray();
        AssertDuration(p1, store);
        AssertDuration(p2, store);
        AssertDuration(p3, store);
        AssertDuration(p4, store);
        AssertDuration(p5, store);


        // Assert 
        var orderByEndTime =
            store.Where(e => e.logEvent == TaskLogger.LogEvent.End)
                .OrderBy(e => e.eventAt)
                .ToImmutableArray();
        
        Assert.Equal(p3.TaskId, orderByEndTime.ElementAt(0).taskId);
        Assert.Equal(p1.TaskId, orderByEndTime.ElementAt(1).taskId);
        Assert.Equal(p4.TaskId, orderByEndTime.ElementAt(2).taskId);
        Assert.Equal(p5.TaskId, orderByEndTime.ElementAt(3).taskId);
        Assert.Equal(p2.TaskId, orderByEndTime.ElementAt(4).taskId);
    }

    async Task DeleyedPublishEvent(LongTaskEvent p, CancellationToken cancellationToken)
    {
        await _producer.PublishAsync(new Event<LongTaskEvent>(p), cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }

    // void AssertStartTime()
    
    void AssertDuration(LongTaskEvent p, (Guid taskId, TaskLogger.LogEvent logEvent, DateTimeOffset eventAt)[] store)
    {
        var tolerance = TimeSpan.FromMilliseconds(20);
        
        var start = store.Single(log => log.taskId == p.TaskId && log.logEvent == TaskLogger.LogEvent.Start);
        var end = store.Single(log => log.taskId == p.TaskId && log.logEvent == TaskLogger.LogEvent.End);

        var duration = end.eventAt - start.eventAt;
        
        Assert.True(
            duration <= (p.SleepTime + tolerance) &&
            duration >= (p.SleepTime - tolerance)
            , $"Expected task duration [{p.SleepTime.TotalMilliseconds}] - Start at [{start.eventAt.UtcDateTime.TimeOfDay.TotalMilliseconds}] - End at [{end.eventAt.UtcDateTime.TimeOfDay.TotalMilliseconds}] => diff [{duration.TotalMilliseconds}]");
    }
    
}