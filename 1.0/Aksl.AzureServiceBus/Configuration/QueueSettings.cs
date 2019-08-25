using System;
using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;

namespace Aksl.AzureServiceBus
{
    public class QueueSettings
    {
        public QueueSettings()
        {
        }

        /// <summary>
        /// sb://<your service namespace>.servicebus.windows.net/; SharedAccessKeyName=<key name>;SharedAccessKey=<your key>                          
        /// </summary>
        public string ServiceBusConnectionStrings { get; set; }

        public ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder{ get; set; }

        public string QueueName => ServiceBusConnectionStringBuilder.EntityPath;

        public MessageHandlerSettings MessageHandlerSettings { get; set; }
    }

    public class MessageHandlerSettings
    {
        public MessageHandlerSettings()
        {
        }

        public int MaxConcurrentCalls { get; set; } = 1;

        public bool AutoComplete { get; set; } = true;

        public TimeSpan MaxAutoRenewDuration { get; set; } = new TimeSpan(5);

    }
}
