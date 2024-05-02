namespace MoonlyBird.InMemoryEventBus.Abstract.Model;

public interface IConsumerCollection : IReadOnlyCollection<IConsumer>
{
    /// <summary>
    ///     Start all consumers
    /// </summary>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken = default);
}