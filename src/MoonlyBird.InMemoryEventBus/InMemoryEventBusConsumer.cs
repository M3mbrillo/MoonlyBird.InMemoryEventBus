using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;
using MoonlyBird.InMemoryEventBus.Abstract.ScopeContext;

namespace MoonlyBird.InMemoryEventBus;

/// <summary>
///     Manage the workflow consumer of a specific event.
/// </summary>
/// <typeparam name="T">type of the event</typeparam>
internal sealed class InMemoryEventBusConsumer<T> : IConsumer<T>
{
    private readonly ChannelReader<Event<T>> _bus;
    private readonly IServiceScopeFactory _serviceProvider;
    private readonly ILogger<InMemoryEventBusConsumer<T>> _logger;
    
    // private IScopeContextEventBus<T>? ScopeContextEventBus { get; set; }
    private IScopeContextMessage<T>? ScopeContextMessage { get; set; }

    /// <summary>
    ///     Prevent overlapping starting
    /// </summary>
    private readonly SemaphoreSlim _semaphoreStarting = new(1, 1);
    
    /// <summary>
    ///     Flag to prevent a double starting
    /// </summary>
    private bool _isRunning;
    
    /// <summary>
    ///     Principal Task that read all events 
    /// </summary>
    private ConfiguredTaskAwaitable _mainTask;
    
    /// <summary>
    ///     Principal CancellationToken that stop all child thread
    /// </summary>
    private CancellationTokenSource? _stoppingToken;

    public InMemoryEventBusConsumer(
        ChannelReader<Event<T>> bus,
        IServiceScopeFactory serviceProvider,
        ILogger<InMemoryEventBusConsumer<T>> logger
    )
    {
        _logger = logger;
        _bus = bus;
        _serviceProvider = serviceProvider;
    }
    
    public async ValueTask Start(CancellationToken token = default)
    {
        EnsureStoppingTokenIsCreated(token);
        await _semaphoreStarting.WaitAsync(token);
        
        try
        {
            if (_isRunning)
                throw new InvalidOperationException("Already running");
            
            
            _mainTask = Task.Run(
                    async () =>
                    {
                        var continuousChannelIterator = _bus.ReadAllAsync(_stoppingToken.Token).ConfigureAwait(false);

                        using var scopeChannelIterrator = _serviceProvider.CreateScope();
                        
                        await foreach (var messageEvent in continuousChannelIterator)
                        {
                            if (_stoppingToken.IsCancellationRequested)
                                break;
                            
                            var scopeContextMessage = scopeChannelIterrator.ServiceProvider.GetRequiredService<IScopeContextMessage<T>>();
                            
                            scopeContextMessage.DeferredHandlersExecution(messageEvent, _stoppingToken.Token);
                        }
                    },
                _stoppingToken.Token
                )
                .ContinueWith((processingTask, o) =>
                {
                    var logger = o as ILogger<InMemoryEventBusConsumer<T>>;

                    logger?.LogInformation("InMemoryEventBusConsumer for events [{event}] is ending!",
                        typeof(T).FullName);

                    if (processingTask.IsCanceled)
                        logger?.LogWarning("InMemoryEventBusConsumer for events [{event}] was canceled",
                            typeof(T).FullName);

                    if (processingTask.IsFaulted)
                        logger?.LogCritical(processingTask.Exception,
                            "InMemoryEventBusConsumer for events [{event}] end as result of a unhandled exception. the task is Faulted",
                            typeof(T).FullName);
                        
                }, _logger, _stoppingToken.Token)
                .ConfigureAwait(false);
        }
        finally
        {
            _semaphoreStarting.Release();
        }
    }


    private readonly object _lockStoppingTokenCreation = new();

    [MemberNotNull(nameof(_stoppingToken))]
    private void EnsureStoppingTokenIsCreated(CancellationToken token = default)
    {
        lock (_lockStoppingTokenCreation)
        {
            if (_stoppingToken is not null && _stoppingToken.IsCancellationRequested == false)
            {
                _stoppingToken.Cancel();
            }

            _stoppingToken = token.CanBeCanceled
                ? CancellationTokenSource.CreateLinkedTokenSource(token)
                : new CancellationTokenSource();
        }
    }
    
    
    public async ValueTask Stop()
    {
        if (_stoppingToken is not null)
            await _stoppingToken!.CancelAsync();
        
        await DisposeAsync().ConfigureAwait(false);
        _isRunning = false;
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_stoppingToken is not null)
            await _stoppingToken!.CancelAsync();
    }

    public void Dispose()
    {
        _semaphoreStarting.Dispose();
        _stoppingToken?.Dispose();
    }
}