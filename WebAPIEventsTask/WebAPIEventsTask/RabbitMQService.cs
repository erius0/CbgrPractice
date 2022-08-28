using RabbitMQ.Client;
using System.Text;

namespace WebAPIEventsTask
{
    public class RabbitMQService : IMessageService
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionFactory = new() { Uri = new(_configuration.GetConnectionString("RabbitMQ")) };
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "msg-queue",
                           durable: false,
                           exclusive: false,
                           autoDelete: false);
        }

        public string? ReceiveMessage()
        {
            var result = _channel.BasicGet("msg-queue", true);
            if (result is null) return null;
            var body = result.Body.ToArray();
            var msg = Encoding.UTF8.GetString(body);
            return msg;
        }

        public void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "",
                           routingKey: "msg-queue",
                           body: body);
        }
    }
}
