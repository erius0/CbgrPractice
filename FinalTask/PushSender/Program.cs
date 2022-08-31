using PushSender;
using PushShared;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IPushService, RabbitPushService>();
        services.AddHostedService<PushSenderWorker>();
    })
    .Build();

await host.RunAsync();
