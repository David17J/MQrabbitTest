using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Exceptions;
using Microsoft.Extensions.Logging;

namespace MQrabbitTest.Services // <--- an deinen Projektnamespace anpassen!
{
    public class RabbitMqService
    {
        private readonly ConnectionFactory _factory;
        private readonly ILogger<RabbitMqService> _logger;
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        public RabbitMqService(ILogger<RabbitMqService> logger)
        {
            _logger = logger;
            _factory = new ConnectionFactory() { 
                HostName = "rabbitmq",
                UserName = "guest",
                Password = "guest",
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
    
            // Wenn du aus Docker heraus zugreifst: HostName = "host.docker.internal"
            // Wenn beide Container im selben Netz: HostName = "rabbitmq"
        }

        public async Task<bool> SendMessageAsync(string queue, string message, string exchange = "", string routingKey = "")
        {
            int retryCount = 0;
            while (retryCount < MaxRetries)
            {
                try
                {
                    using var connection = _factory.CreateConnection();
                    using var channel = connection.CreateModel();

                    // Declare queue with more durability options
                    channel.QueueDeclare(
                        queue: queue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true; // Message persistence
                    properties.DeliveryMode = 2; // Persistent

                    var body = Encoding.UTF8.GetBytes(message);
                    
                    channel.BasicPublish(
                        exchange: exchange,
                        routingKey: string.IsNullOrEmpty(routingKey) ? queue : routingKey,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);

                    _logger.LogInformation($"Message sent successfully to queue: {queue}");
                    return true;
                }
                catch (BrokerUnreachableException ex)
                {
                    _logger.LogWarning($"RabbitMQ broker unreachable. Attempt {retryCount + 1} of {MaxRetries}. Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending message to queue {queue}. Attempt {retryCount + 1} of {MaxRetries}. Error: {ex.Message}");
                }

                retryCount++;
                if (retryCount < MaxRetries)
                {
                    await Task.Delay(RetryDelayMs * retryCount);
                }
            }

            return false;
        }
    }
}
