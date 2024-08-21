using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;
using MoonlyBird.InMemoryEventBus.Test.RacingLog.Model;
using MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride.Model;

namespace MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride;

public class TestCase
{
    private Setup _setup = new();

    
    [Fact]
    async Task CheckUniqueScope()
    {
        CancellationToken cancellationToken = CancellationToken.None;

        EventStore.InitStore(new ());
        var consumer = _setup.GetConsumer<DummyEvent>();
        var producer = _setup.GetProducer<DummyEvent>();
        
        await consumer.Start(cancellationToken);

        var firstRewriteThenClear = new Event<DummyEvent>(new DummyEvent()
        {
            DelayClearHandler = TimeSpan.FromSeconds(1)
        });

        await producer.PublishAsync(firstRewriteThenClear, cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        
        var firstClearThenRewrite = new Event<DummyEvent>(new DummyEvent()
        {
            DelayRewriteHandler = TimeSpan.FromSeconds(1)
        });
        await producer.PublishAsync(firstClearThenRewrite, cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        var store = EventStore.GetStore();
        
        Assert.Empty( store
            .GroupBy(x => x.serviceScope)
            .Where(x => x.Count() > 1));
    }
}