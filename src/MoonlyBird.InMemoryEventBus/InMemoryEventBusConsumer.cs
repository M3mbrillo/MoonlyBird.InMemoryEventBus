using System.Linq.Expressions;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus;

internal sealed class InMemoryEventBusConsumer<T> : IConsumer<T>
{
    private readonly ChannelReader<Event<T>> _bus;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InMemoryEventBusConsumer<T>> _logger;

    public InMemoryEventBusConsumer(
        ChannelReader<Event<T>> bus,
        IServiceScopeFactory scopeFactory,
        ILogger<InMemoryEventBusConsumer<T>> logger
    )
    {
        _logger = logger;
        _bus = bus;
        _scopeFactory = scopeFactory;
    }
    

    private CancellationTokenSource? _stoppingToken;
    
    public async ValueTask Start(CancellationToken token = default)
    {
        EnsureStoppingTokenIsCreated(token);

        // factory new scope so we can use it as execution context
        await using var scope = _scopeFactory.CreateAsyncScope();

        // retrieve scoped dependencies
        var handlers = scope.ServiceProvider
                                            .GetServices<IEventHandler<T>>()
                                            .ToList();
        var metadataAccessor = scope.ServiceProvider.GetRequiredService<IEventContextAccessor<T>>();

        if (handlers.Count == 0)
        {
            _logger.LogWarning("No handlers defined for event of {type}", typeof(T).Name);
            return;
        }

        // run the processing
        Task.Run(
            async () => await StartProcessing(handlers, metadataAccessor).ConfigureAwait(false),
            _stoppingToken!.Token
        ).ConfigureAwait(false);
    }

    private void EnsureStoppingTokenIsCreated(CancellationToken token = default)
    {
        if (_stoppingToken is not null && _stoppingToken.IsCancellationRequested == false)
        {
            _stoppingToken.Cancel();
        }
        
        _stoppingToken = token.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(token) : new CancellationTokenSource();
    }
    
    /// <summary>
    /// Subscribes to channel changes and triggers event handling
    /// </summary>
    private async ValueTask StartProcessing(List<IEventHandler<T>> handlers, IEventContextAccessor<T> contextAccessor)
    {
        var continuousChannelIterator = _bus.ReadAllAsync(_stoppingToken!.Token)
            .WithCancellation(_stoppingToken.Token)
            .ConfigureAwait(false);

        await foreach (var task in continuousChannelIterator)
        {
            if (_stoppingToken.IsCancellationRequested)
                break;

            // invoke handlers in parallel
            await Parallel.ForEachAsync(
                handlers,
                _stoppingToken.Token,
                async (handler, scopedToken) => await ExecuteHandler(handler, task, contextAccessor, scopedToken).ConfigureAwait(false)
            ).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Executes the handler in async scope
    /// </summary>
    private ValueTask ExecuteHandler(IEventHandler<T> handler, Event<T> task, IEventContextAccessor<T> ctx, CancellationToken token)
    {
        ctx.Set(task); // set metadata and begin scope
        using var logScope = _logger.BeginScope(task.Metadata ?? new EventMetadata(Guid.NewGuid().ToString()));

        Task.Run(
            async () => await handler.Handle(task.Data, token), token
        ).ConfigureAwait(false);

        return ValueTask.CompletedTask;
    }
    
    
    
    public async ValueTask Stop(CancellationToken _ = default)
    {
        await DisposeAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        _stoppingToken?.Cancel();
    }
}