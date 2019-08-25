using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Aksl.AzureServiceBus
{
    #region IBus
    /// <summary>
    /// Bus Interface
    /// </summary>
    public interface IAzureQueueService
    {
        #region Properties
        ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder
        {
            get;
        }

        QueueClient QueueClient
        {
            get;
        }
        #endregion

        #region Methods
        IAzureQueueSender CreateQueueSender();

        void CreateQueueClient();

        Task CloseAsync();
        #endregion
    }
    #endregion
}
