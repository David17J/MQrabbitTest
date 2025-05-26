using RabbitMQ.Client;
using System.Text;

namespace MQrabbitTest.Services // <--- an deinen Projektnamespace anpassen!
{
    public class RabbitMqService
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqService()
        {
            // Passe hier den HostName an!
            _factory = new ConnectionFactory() { 
                HostName = "localhost" ,
                UserName = "guest",
                Password = "guest"
            
            };
    
            // Wenn du aus Docker heraus zugreifst: HostName = "host.docker.internal"
            // Wenn beide Container im selben Netz: HostName = "rabbitmq"
        }

        public async void SendMessage(string queue, string message)
        {
            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();


            await channel.QueueDeclareAsync(queue: queue, durable: false, exclusive: false, autoDelete: false,
    arguments: null);

           // channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
            //Console.WriteLine(body);

            // channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
        }
    }
}
