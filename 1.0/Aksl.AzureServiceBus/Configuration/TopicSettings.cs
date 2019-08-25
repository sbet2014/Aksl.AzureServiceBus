namespace Aksl.AzureServiceBus
{
    public class TopicSettings
    {
        public TopicSettings()
        {
        }

        //https://<your service namespace>.servicebus.windows.net/<topic name>/subscriptions/<subscription name>
        public string ConnectionString { get; set; }

        public string TopicPath { get; set; }

        public string SubscriptionName { get; set; }

        public double OperationTimeout { get; set; }
    }
}
