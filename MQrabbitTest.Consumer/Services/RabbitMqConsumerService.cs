using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MQrabbitTest.Consumer.Services
{
    public class RabbitMqConsumerService : BackgroundService
    {
        private readonly ILogger<RabbitMqConsumerService> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string QueueName = "test";
        private const int MaxRetries = 3;

        public RabbitMqConsumerService(ILogger<RabbitMqConsumerService> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = "rabbitmq", // Docker service name
                UserName = "guest",
                Password = "guest",
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare the queue with the same settings as the producer
                _channel.QueueDeclare(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Configure QoS
                _channel.BasicQos(
                    prefetchSize: 0,
                    prefetchCount: 1,
                    global: false);

                _logger.LogInformation("RabbitMQ Consumer Service initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing RabbitMQ consumer: {ex.Message}");
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Received message: {message}");

                    // Process the message here
                    ProcessMessage(message);

                    // Acknowledge the message
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing message: {ex.Message}");
                    
                    // Reject the message and requeue if not exceeded max retries
                    var retryCount = GetRetryCount(ea);
                    if (retryCount < MaxRetries)
                    {
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                        _logger.LogWarning($"Message requeued. Retry attempt {retryCount + 1}");
                    }
                    else
                    {
                        // Reject without requeue if max retries exceeded
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        _logger.LogError("Message rejected after maximum retry attempts");
                    }
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);

            return Task.CompletedTask;
        }

        private int GetRetryCount(BasicDeliverEventArgs ea)
        {
            var headers = ea.BasicProperties.Headers;
            if (headers != null && headers.TryGetValue("x-retry-count", out var retryCountObj))
            {
                return (int)retryCountObj;
            }
            return 0;
        }

        private void ProcessMessage(string message)
        {
            // Add your message processing logic here
            _logger.LogInformation($"Processing message: {message}");
            // Simulate some work
            Thread.Sleep(1000);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
} 