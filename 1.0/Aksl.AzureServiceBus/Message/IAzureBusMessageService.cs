using System;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Aksl.AzureServiceBus
{
    #region IBus
    /// <summary>
    ///Azure BusMessage Service
    /// </summary>
    public interface IAzureBusMessageService
    {
        #region Properties
        ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder
        {
            get;
        }

        IMessageSender InnerMessageSender
        {
            get;
        }
        #endregion

        #region IMessageSender Methods
        IAzureBusMessageSender CreateMessageSender();
        #endregion

        #region IMessageReceiver Methods
        IAzureBusMessageReciever CreateMessageReceiver(); 
        #endregion
    }
    #endregion
}
