using PushShared.Push.Data;

namespace PushShared
{
    public interface IPushService
    {
        public void SendPush(PushNotification push);
        public PushNotification? ReceivePush();
    }
}
