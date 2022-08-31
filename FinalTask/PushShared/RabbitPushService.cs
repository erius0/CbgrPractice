using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PushShared.Push.Data;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PushShared
{
    public class RabbitPushService : IPushService
    {
        private readonly ILogger<RabbitPushService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitPushService(ILogger<RabbitPushService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionFactory = new() { Uri = new(_configuration.GetConnectionString("RabbitMQ")!) };
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "push-queue",
                           durable: false,
                           exclusive: false,
                           autoDelete: false);
        }

        public void SendPush(PushNotification push)
        {
            _logger.LogInformation("Serializing push notification...");
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(push));
            _logger.LogInformation("Publishing push notification to queue");
            _channel.BasicPublish(exchange: "",
                           routingKey: "push-queue",
                           body: body);
            _logger.LogInformation("Publish successful");
        }

        public PushNotification? ReceivePush()
        {
            var result = _channel.BasicGet("push-queue", true);
            if (result is null)
            {
                return null;
            }
            _logger.LogInformation("Push notification found, deserializing...");
            var body = result.Body.ToArray();
            var push = JsonSerializer.Deserialize<PushNotification>(Encoding.UTF8.GetString(body));
            _logger.LogInformation("Get successful, returning push notification");
            return push;
        }
    }
}
