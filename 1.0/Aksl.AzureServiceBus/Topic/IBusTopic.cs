using System;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Aksl.AzureServiceBus
{
    #region IBusTopic
    public interface IBusTopic
    {
        #region Properties
        ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder
        {
            get;
        }

        TopicClient Client
        {
            get;
        }
        #endregion

        #region IMessageSender Methods
        IBusMessageSenderClient CreateMessageSender();
        #endregion

    }
    #endregion
}
