namespace WebAPIEventsTask
{
    public interface IMessageService
    {
        public void SendMessage(string message);
        public string? ReceiveMessage();
    }
}
