using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace WebAPIEventsTask.Controllers
{
    [ApiController]
    [Route("api")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IConfiguration _configuration;
        private static IModel rmqChannel;
        private static readonly string QUEUE_NAME = "test-queue";

        static EventController()
        {
            var connectionFactory = new ConnectionFactory { HostName = "localhost" };
            var connection = connectionFactory.CreateConnection();
            rmqChannel = connection.CreateModel();
        }

        public EventController(ILogger<EventController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            rmqChannel.QueueDeclare(QUEUE_NAME, false, false, false);
        }

        [HttpPost("send")]
        public IActionResult SendMessage(string msg)
        {
            rmqChannel.BasicPublish("", QUEUE_NAME, null, Encoding.UTF8.GetBytes(msg));
            return Ok();
        }

        [HttpGet("receive")]
        public IActionResult ReceiveMessage()
        {
            var consumer = new EventingBasicConsumer(rmqChannel);
            string? msg = null;
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                msg = Encoding.UTF8.GetString(body);
            };
            rmqChannel.BasicConsume(QUEUE_NAME, true, consumer);
            return msg == null ? NotFound() : Ok(msg);
        }
    }
}
