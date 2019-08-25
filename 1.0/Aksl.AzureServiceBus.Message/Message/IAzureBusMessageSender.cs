using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Aksl.AzureServiceBus.Message
{
    #region IAzureMessageSender
    /// <summary>
    /// Azure Bus Message Sender Interface
    /// </summary>
    public interface IAzureBusMessageSender
    {
        #region Properties
        IMessageSender MessageSender { get; }

        Action<MessageContext> OnSendCallBack { get; set; }
        #endregion

        #region Send Methods
        /// <summary>
        /// Send
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Task</returns>
        Task SendAsync(Microsoft.Azure.ServiceBus.Message message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send Object to queue, as json
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Task</returns>
        Task SendAsync(object obj, CancellationToken cancellationToken = default);
        #endregion

        #region SendBatch Methods
        /// <summary>
        /// Send Batch
        /// </summary>
        /// <param name="message">Messages</param>
        /// <returns>Task</returns>
        Task SendBatchAsync(IEnumerable<Microsoft.Azure.ServiceBus.Message> messages, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send Object to queue, as json
        /// </summary>
        /// <param name="messages">Messages</param>
        /// <returns>Task</returns>
        Task SendBatchAsync(IEnumerable<object> messages, CancellationToken cancellationToken = default);
        #endregion

        #region  Close Methods
        Task CloseAsync();
        #endregion
    }
    #endregion
}