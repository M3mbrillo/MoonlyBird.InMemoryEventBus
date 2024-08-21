using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride.Model;

namespace MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride.EventHandler;

public class RewriteModelEventHandler : IEventHandler<Model.DummyEvent>
{
    private const string HandlerName = "RewriteModelEventHandler";

    private readonly IDummyExternalService _dummyExternalService;

    public RewriteModelEventHandler(IDummyExternalService dummyExternalService)
    {
        _dummyExternalService = dummyExternalService;
    }

    public async ValueTask Handle(DummyEvent? dataEvent, CancellationToken token = default)
    {
        // Force processed after o before that ClearModel
        await Task.Delay(dataEvent!.DelayRewriteHandler, CancellationToken.None);

        if (string.IsNullOrEmpty(dataEvent!.AnyData))
            throw new Exception("Event was modified from another handler");

        dataEvent.AnyData = Guid.NewGuid().ToString();
        
        EventStore.SaveEvent(HandlerName, dataEvent.AnyData, _dummyExternalService.ScopeIdentifier);
    }
}