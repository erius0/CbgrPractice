using PushShared;

namespace PushSender
{
    public class PushSenderWorker : BackgroundService
    {
        private readonly ILogger<PushSenderWorker> _logger;
        private readonly IPushService _pushService;

        public PushSenderWorker(ILogger<PushSenderWorker> logger, IPushService pushService)
        {
            _logger = logger;
            _pushService = pushService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Waiting for a push notification to send...");
            while (!stoppingToken.IsCancellationRequested)
            {
                var push = _pushService.ReceivePush();
                if (push is not null)
                {
                    _logger.LogInformation("Sending push notifications with title {Title}...", push.Title);
                    await Task.Delay(100, stoppingToken);
                    _logger.LogInformation("Push notification successfully sent");
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}