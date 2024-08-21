using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride.Model;

namespace MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride.EventHandler;

public class ClearModelEventHandler : IEventHandler<Model.DummyEvent>
{
    private const string HandlerName = "ClearModelEventHandler";
    
    private readonly IDummyExternalService _dummyExternalService;

    public ClearModelEventHandler(IDummyExternalService dummyExternalService)
    {
        _dummyExternalService = dummyExternalService;
    }
    
    
    public async ValueTask Handle(DummyEvent? dataEvent, CancellationToken token = default)
    {
        // Force processed after o before that ClearModel
        await Task.Delay(dataEvent!.DelayClearHandler, CancellationToken.None);

        if (dataEvent!.AnyData != DummyEvent.DefaultValue)
            throw new Exception("Event was modified before from another handler");

        dataEvent!.AnyData = string.Empty;

        EventStore.SaveEvent(HandlerName, dataEvent.AnyData, _dummyExternalService.ScopeIdentifier);
    }
}