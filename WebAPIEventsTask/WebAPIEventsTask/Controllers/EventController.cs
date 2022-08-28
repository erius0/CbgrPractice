using Microsoft.AspNetCore.Mvc;

namespace WebAPIEventsTask.Controllers
{
    [ApiController]
    [Route("api")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMessageService _messageService;

        public EventController(ILogger<EventController> logger, IConfiguration configuration, IMessageService messageService)
        {
            _logger = logger;
            _configuration = configuration;
            _messageService = messageService;
        }

        [HttpPost("send")]
        public IActionResult SendMessage(string msg)
        {
            _messageService.SendMessage(msg);
            return Ok();
        }

        [HttpGet("receive")]
        public IActionResult ReceiveMessage()
        {
            string? msg = _messageService.ReceiveMessage();
            return msg == null ? NotFound() : Ok(msg);
        }
    }
}
