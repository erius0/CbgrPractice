using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPITask.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class EmailController : ControllerBase
    {
        private static readonly IDictionary<string, int> emailMessageStats = new Dictionary<string, int>();

        private readonly ILogger<EmailController> _logger;

        public EmailController(ILogger<EmailController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sends a message to specified email addresses
        /// </summary>
        /// <param name="request">A list of emails and a message to send</param>
        /// <response code="200">The message was successfully sent</response>
        /// <response code="400">If one of the emails isn't a valid email address</response>
        [HttpPost("send")]
        public IActionResult SendMessages(SendEmailsRequestModel request)
        {
            _logger.LogInformation("Sending the message: {Message}...", request.Message);
            foreach (var email in request.Emails)
            {
                _logger.LogInformation("Trying to send the message to {email}", email);
                if (emailMessageStats.ContainsKey(email)) emailMessageStats[email]++;
                else emailMessageStats[email] = 1;
                _logger.LogInformation("Message sent successfully");
            }
            _logger.LogInformation("The message was successfully sent to all targets");
            return Ok();
        }

        /// <summary>
        /// Gets how many messages has every email address received
        /// </summary>
        /// <returns>Received messages statistic for every email address</returns>
        /// <response code="200">The statistics were successfully fetched</response>
        /// <response code="204">No one has received any messages at all</response>
        [HttpGet("stat")]
        public IActionResult GetMessageCountAll()
        {
            _logger.LogInformation("Getting the overall recieved messages statistics...");
            var messageStats = new List<EmailStatResponseModel>();
            foreach (var (email, msgCount) in emailMessageStats)
            {
                messageStats.Add(new() { Email = email, Message = msgCount });
            }
            _logger.LogInformation("The statistics were successfully fetched");
            return messageStats.Count > 0 ? Ok(messageStats) : NoContent();
        }

        /// <summary>
        /// Gets how many messages has a specified email address received
        /// </summary>
        /// <param name="email">The email address in question</param>
        /// <returns>Amount of messages that were sent to specified email</returns>
        /// <response code="200">The statistics were successfully fetched</response>
        /// <response code="400">The specified email isn't a valid email address</response>
        /// <response code="404">The specified email hasn't recieved any messages</response>
        [HttpGet("count/{email}")]
        public IActionResult GetMessageCount([EmailAddress] string email)
        {
            _logger.LogInformation("Getting the recieved messages statistics for {email}", email);
            var retrieved = emailMessageStats.TryGetValue(email, out var result);
            if (retrieved)
            {
                _logger.LogInformation("The statistics were successfully fetched");
                return Ok(result);
            }
            _logger.LogInformation("The statistics for {email} were not found", email);
            return NotFound();
        }

        public class SendEmailsRequestModel
        {
            [Required, EnumerableEmails]
            public IEnumerable<string> Emails { get; set; }
            [Required]
            public string Message { get; set; }
        }
        public record EmailStatResponseModel
        {
            [Required, EmailAddress]
            public string Email { get; set; }
            [Required]
            public int Message { get; set; }
        }

        public class EnumerableEmailsAttribute : ValidationAttribute
        {
            public override bool IsValid(object? value)
            {
                if (value == null) return false;
                if (value is not IEnumerable<string> emails) return false;
                var emailAttribute = new EmailAddressAttribute();
                foreach (var email in emails)
                {
                    if (!emailAttribute.IsValid(email))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}