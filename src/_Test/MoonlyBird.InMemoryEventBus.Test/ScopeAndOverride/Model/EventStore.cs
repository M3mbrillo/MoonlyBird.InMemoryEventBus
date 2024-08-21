using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace MoonlyBird.InMemoryEventBus.Test.ScopeAndOverride.Model;

public static class EventStore
{
    private static readonly AsyncLocal<ConcurrentBag<(string sourceHandler, string dataValue, Guid serviceScope)>> _store =
        new();
    
    private static ConcurrentBag<(string sourceHandler, string dataValue, Guid serviceScope)> Store => _store.Value ?? throw new NullReferenceException("Call EventStore.InitStore in your FACT test to ensure dont shared the same data between test");
    
    public static void InitStore(ConcurrentBag<(string sourceHandler, string dataValue, Guid serviceScope)> store)
        => _store.Value = store;


    public static void SaveEvent(string sourceHandler, string dataValue, Guid serviceScope)
    {
        Store.Add((sourceHandler, dataValue, serviceScope));
    }

    public static (string sourceHandler, string dataValue, Guid serviceScope)[] GetStore()
        => Store.ToArray();
}