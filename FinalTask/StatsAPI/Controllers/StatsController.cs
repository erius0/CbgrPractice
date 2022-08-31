using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PushShared;
using PushShared.Mobile.Data;
using System.ComponentModel.DataAnnotations;

namespace StatsAPI.Controllers
{
    [ApiController]
    [Route("stats")]
    public class StatsController : ControllerBase
    {
        private readonly ILogger<StatsController> _logger;
        private readonly MobileContext _context;

        public StatsController(ILogger<StatsController> logger, MobileContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("user_apps")]
        public IActionResult GetUserApps()
        {
            _logger.LogInformation("Fetching the user apps data...");
            var userAppStats = new
            {
                AppUsers = new List<MobileAppUser>(),
                VersionRegistrations = new Dictionary<string, int>(),
                VersionUniquePhones = new Dictionary<string, int>()
            };
            _logger.LogInformation("Getting all user apps from databse...");
            var appUsers = _context.MobileAppUsers.Select(u => u).AsEnumerable();
            userAppStats.AppUsers.AddRange(appUsers);
            _logger.LogInformation("Calculating registrations and unique phone numbers for each version...");
            var versions = appUsers.Select(u => u.Version).ToHashSet();
            foreach (var version in versions)
            {
                userAppStats.VersionRegistrations[version] = appUsers.Count(u => u.Version == version);
                userAppStats.VersionUniquePhones[version] = appUsers.DistinctBy(u => u.Phone).Count(u => u.Version == version);
            }
            _logger.LogInformation("Successfully fetched the data");
            return Ok(userAppStats);
        }

        [HttpGet("messages")]
        public IActionResult GetMessages([Phone] string phone)
        {
            _logger.LogInformation("Fetching the messages for phone number {phone}...", phone);
            var messageStats = new { Messages = new List<Message>() };
            messageStats.Messages.AddRange(_context.Messages.Where(m => m.Phone == phone).Select(m => m));
            if (!messageStats.Messages.Any())
            {
                _logger.LogWarning("Phone number {phone} doesn't have any messages", phone);
                return NotFound("The specefied number hasn't received any push notifications yet");
            }
            _logger.LogInformation("Successfully fetched the messages");
            return Ok(messageStats);
        }
    }
}
