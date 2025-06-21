namespace OrderOrchestrator.Domain.Interfaces
{
    public interface IMessageBus
    {
        Task Publish(string queue, string message);
    }
}
