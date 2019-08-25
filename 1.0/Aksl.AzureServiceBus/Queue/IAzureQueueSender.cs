using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Aksl.AzureServiceBus
{
    #region IAzureQueueSender
    /// <summary>
    /// Azure Queue Sender Interface
    /// </summary>
    public interface IAzureQueueSender
    {
        #region Properties
        QueueClient QueueClient { get; set; }

        Action<MessageContext> OnSendCallBack { get; set; }
        #endregion

        #region SendBatch Methods
        /// <summary>
        /// Send Batch
        /// </summary>
        /// <param name="message">Messages</param>
        /// <returns>Task</returns>
        Task SendBatchAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default);
        #endregion

        #region  Close Methods
        Task CloseAsync();
        #endregion
    }
    #endregion
}
