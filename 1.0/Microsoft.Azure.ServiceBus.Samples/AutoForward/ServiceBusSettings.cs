
namespace AutoForward
{
    public class ServiceBusSettings
    {
        public ServiceBusSettings()
        {
        }

        /// <summary>
        /// sb://<your service namespace>.servicebus.windows.net/; SharedAccessKeyName=<key name>;SharedAccessKey=<your key>                          
        /// </summary>
        public string ConnectionString { get; set; }

        public string NamespaceConnectionString { get; set; }

        public string EntityPath { get; set; }

        public double OperationTimeout { get; set; }
    }
}
