namespace OrderOrchestrator.Infrastructure.Configurations
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; }
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }
    }

}
