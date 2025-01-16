using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonlyBird.InMemoryEventBus.Abstract;
using MoonlyBird.InMemoryEventBus.Abstract.Model;

namespace MoonlyBird.InMemoryEventBus.ScopeContext;

public abstract class BaseScopeContext<TEvent> : IScopeContext<TEvent>
{
    internal static string GenerateServiceHandlerKey(EnumScopeContext scopeContext)
    {
        var keyedPrefix = scopeContext.ToString();
        var eventName = typeof(TEvent).Name;

        return keyedPrefix + "_" + eventName;
    }

    protected abstract EnumScopeContext ScopeContextType { get; }
    protected abstract IServiceScopeFactory ScopeFactory { get; }
    protected abstract ILogger Logger { get; }

    // TODO: Chequear y validar los contexto de ejecusion si se tienen que eliminar o no
    static protected ConcurrentBag<Task> RunningDeferredHandlersExecutions { get; } = new();

    // TODO: Chequear y validar los handlers que tengo corriendo terminan y se tienen que eliminar
    protected ConcurrentBag<ConfiguredTaskAwaitable> RunningHandles { get; } = new();

    public virtual void DeferredHandlersExecution(Event<TEvent> @event, CancellationToken cancellationToken = default)
    {
        var longRunningTask = SpawnExecutionContext(@event, cancellationToken);

        RunningDeferredHandlersExecutions.Add(longRunningTask);
    }

    protected virtual async Task SpawnExecutionContext(Event<TEvent> @event,
        CancellationToken cancellationToken = default)
    {
        using var scope = ScopeFactory.CreateScope();

        var handlers = this.GetEventHandlers(scope.ServiceProvider);

        // Todos mis handlers comparten la misma metadata del mensaje...
        // esto puede cambiar en un futuro con el patron envelop y manejar un ReTry y DLQ
        var eventContextAccessor = scope.ServiceProvider.GetRequiredService<IEventContextAccessor<TEvent>>();
        eventContextAccessor.Set(@event);

        foreach (var handler in handlers)
        {
            var task = Task.Run(
                    async () => { await handler.Handle(@event.Data, cancellationToken); },
                    cancellationToken
                )
                .ContinueWith(
                    (processingTask, o) =>
                    {
                        if (o is not (ILogger logger, IEventHandler<TEvent> handler, Event<TEvent> @event,
                            CancellationToken cancellationToken)) return;

                        var handlerName = handler.GetType().FullName;

                        logger?.LogInformation("Handle {handle} for events [{event}] is end!",
                            handlerName,
                            typeof(TEvent).FullName);

                        if (processingTask.IsCanceled)
                        {
                            logger?.LogWarning("Handle {handle} for events [{event}] was canceled!",
                                handlerName,
                                typeof(TEvent).FullName);

                            handler.HandleCancellation(@event, cancellationToken);
                        }

                        if (processingTask.IsFaulted)
                        {
                            logger?.LogError(
                                processingTask.Exception,
                                "Handle {handle} for events [{event}] end as result of a unhandled exception. the task is Faulted",
                                handlerName,
                                typeof(TEvent).FullName);

                            // Si tiras una exception aca la tenes que giga pedir...
                            handler.HandleException(@event, processingTask.Exception, cancellationToken);
                        }
                    },
                    (logger: Logger, handler, @event, cancellationToken)
                    /*,cancellationToken*/
                    /*Si voy a disparar un handler.HandleCancellation(@event, cancellationToken) no puedo cancelar este ultimo proceso*/
                );

            RunningHandles.Add(task.ConfigureAwait(false));
        }
    }

    protected IEnumerable<IEventHandler<TEvent>> GetEventHandlers(IServiceProvider scopeServiceProvider)
    {
        var myKey = GenerateServiceHandlerKey(this.ScopeContextType);
        var handlers = scopeServiceProvider.GetKeyedServices<IEventHandler<TEvent>>(myKey);
        return handlers;
    }
}