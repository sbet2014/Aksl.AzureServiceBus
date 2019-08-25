using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Aksl.AzureServiceBus
{
    #region IBusMessageSender 
    /// <summary>
    /// Bus Message SenderClient Interface
    /// </summary>
    public interface IBusMessageSenderClient
    {
        #region Properties
        ISenderClient MessageSender { get; }

        Action<MessageContext> OnSendCallBack { get; set; }
        #endregion

        #region Send Methods
        /// <summary>
        /// Send
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Task</returns>
        Task SendAsync(Message message, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Send Object to queue, as json
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Task</returns>
        Task SendAsync(object obj, CancellationToken cancellationToken = default(CancellationToken));
        #endregion

        #region SendBatch Methods
        /// <summary>
        /// Send Batch
        /// </summary>
        /// <param name="message">Messages</param>
        /// <returns>Task</returns>
        Task SendBatchAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Send Object to queue, as json
        /// </summary>
        /// <param name="messages">Messages</param>
        /// <returns>Task</returns>
        Task SendBatchAsync(IEnumerable<object> messages, CancellationToken cancellationToken = default(CancellationToken));
        #endregion

        #region  Schedule Methods
        Task<long> ScheduleMessageAsync(Message message, DateTimeOffset scheduleEnqueueTimeUtc, CancellationToken cancellationToken = default(CancellationToken));

        Task CancelScheduledMessageAsync(long sequenceNumber, CancellationToken cancellationToken = default(CancellationToken));
        #endregion

        #region  Close Methods
        Task CloseAsync();
        #endregion
    }
    #endregion
}