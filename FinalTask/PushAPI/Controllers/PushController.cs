using Microsoft.AspNetCore.Mvc;
using PushShared;
using PushShared.Mobile.Data;
using PushShared.Push.Data;

namespace PushAPI.Controllers
{
    [ApiController]
    [Route("push")]
    public class PushController : ControllerBase
    {
        private readonly ILogger<PushController> _logger;
        private readonly MobileContext _context;
        private readonly IPushService _pushService;

        public PushController(ILogger<PushController> logger, MobileContext context, IPushService pushService)
        {
            _logger = logger;
            _context = context;
            _pushService = pushService;
        }

        [HttpPost("create")]
        public IActionResult CreatePush(PushNotification push)
        {
            _logger.LogInformation("Received a request to create a new push notification...");
            _logger.LogInformation("Title: {Title}", push.Title);
            var guids = new List<string>();
            _logger.LogInformation("Fetching GUIDs...");
            foreach (var phone in push.SendToNumbers)
            {
                var guidsForPhone = _context.MobileAppUsers.Where(u => phone == u.Phone).Select(u => u.AppGuid);
                if (!guidsForPhone.Any())
                {
                    _logger.LogWarning("Phone number {phone} isn't registered in the database", phone);
                    continue;
                }
                guids.AddRange(guidsForPhone);
                var msg = new Message() { Phone = phone, Title = push.Title, Contents = push.Message };
                _context.Messages.Add(msg);
            }
            if (!guids.Any())
            {
                var error = "None of the specified numbers are registered in the database, push notification won't be sent";
                _logger.LogWarning(error);
                return NotFound(error);
            }
            _pushService.SendPush(push);
            _logger.LogInformation("Successfully sent push notification to queue");
            _logger.LogInformation("Saving sent messages to database...");
            _context.SaveChanges();
            _logger.LogInformation("Successfully saved the messages");
            return Ok();
        }
    }
}
