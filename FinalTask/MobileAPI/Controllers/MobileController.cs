using Microsoft.AspNetCore.Mvc;
using PushShared;
using PushShared.Mobile.Data;

namespace MobileAPI.Controllers
{
    [ApiController]
    [Route("mobile_apps")]
    public class MobileController : ControllerBase
    {
        private readonly ILogger<MobileController> _logger;
        private readonly MobileContext _mobileContext; 

        public MobileController(ILogger<MobileController> logger, MobileContext mobileContext)
        {
            _logger = logger;
            _mobileContext = mobileContext;
        }

        [HttpPost("register_app")]
        public IActionResult RegisterUser(MobileAppUser user)
        {
            _logger.LogInformation("Registering a new user app...");
            var exists = _mobileContext.MobileAppUsers.Any(u => u.AppGuid == user.AppGuid);
            if (exists)
            {
                _logger.LogWarning("User app with GUID {AppGuid} already exists", user.AppGuid);
                return BadRequest("User app with this GUID already exists");
            }
            _mobileContext.MobileAppUsers.Add(user);
            _mobileContext.SaveChanges();
            _logger.LogInformation("User app successfully registered");
            return Ok();
        }

        [HttpDelete("delete_app")]
        public IActionResult DeleteUser([GuidAttribue] string guid)
        {
            _logger.LogInformation("Deleteing a user app...");
            var exists = _mobileContext.MobileAppUsers.Any(u => u.AppGuid == guid);
            if (!exists)
            {
                _logger.LogWarning("User app with GUID {guid} was not found", guid);
                return NotFound();
            }
            var userToDelete = _mobileContext.MobileAppUsers.First(u => u.AppGuid == guid);
            _mobileContext.MobileAppUsers.Remove(userToDelete);
            _mobileContext.SaveChanges();
            _logger.LogInformation("User app successfully deleted");
            return Ok();
        }
    }
}
