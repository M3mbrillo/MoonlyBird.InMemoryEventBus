using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.DependencyInjection;

public static class InMemoryServiceExtension
{
    public static ConfigureEventBus<TEvent> AddInMemoryEvent<TEvent>(this IServiceCollection services)
    {

        var configureEventBus = new ConfigureEventBus<TEvent>(services);
        
        return configureEventBus;
    }

}