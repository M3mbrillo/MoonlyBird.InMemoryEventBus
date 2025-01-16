using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract.ScopeContext;

namespace MoonlyBird.InMemoryEventBus.ScopeContext;

public class ScopeContextMessage<TEvent>
    : BaseScopeContext<TEvent>, IScopeContextMessage<TEvent>
{
    public ScopeContextMessage(IServiceScopeFactory serviceScopeFactory, ILogger<ScopeContextMessage<TEvent>> logger)
    {
        ScopeFactory = serviceScopeFactory;
        Logger = logger;
    }
    
    protected override EnumScopeContext ScopeContextType => EnumScopeContext.Message;
    protected override IServiceScopeFactory ScopeFactory { get; }
    protected override ILogger Logger { get; }
}