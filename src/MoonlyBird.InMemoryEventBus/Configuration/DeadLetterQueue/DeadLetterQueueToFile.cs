using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract.Configuration;
using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.Configuration.DeadLetterQueue;

public class DeadLetterQueueToFile : IDeadLetterQueueBehavior
{
    private readonly string _pathStorage;
    private readonly ILogger _logger;

    public DeadLetterQueueToFile(string pathStorage, ILogger logger)
    {
        _pathStorage = pathStorage;
        _logger = logger;
        if (!Path.Exists(pathStorage))
            Directory.CreateDirectory(pathStorage);
    }
    
    public async ValueTask HandleRejectedManyTimesEventAsync<T>(Event<T> @event, CancellationToken cancellationToken)
    {
        var uuid = Ulid.NewUlid();
        
        var path = Path.Combine(_pathStorage, uuid.ToString());
        
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(@event), cancellationToken);
    }
}