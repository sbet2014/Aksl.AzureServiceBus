using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Aksl.AzureServiceBus
{
    #region IAzureBusMessageReciever
    /// <summary>
    /// Azur Bus Message Reciever Interface
    /// </summary>
    public interface IAzureBusMessageReciever
    {
        #region Properties
        /// <summary>
        /// Server Wait Time
        /// </summary>
        //TimeSpan ServerWaitTime
        //{
        //    get;
        //}
   //     IMessageReceiver MessageReciever { get; }

        Action<MessageContext> OnRecieveCallBack { get; set; }
        #endregion

        #region Methods
        Task<Message> ReceiveAsync(TimeSpan operationTimeout, CancellationToken cancellationToken = default);

        Task<IList<Message>> ReceiveBatchAsync(TimeSpan operationTimeout, int maxMessageCount = 10, CancellationToken cancellationToken = default);

        //Task<Message> ReceiveBySequenceNumberAsync(long sequenceNumber,CancellationToken cancellationToken = default(CancellationToken));

        //Task<IList<Message>> ReceiveBySequenceNumberAsync(IEnumerable<long> sequenceNumbers, CancellationToken cancellationToken = default(CancellationToken));

      //  Task CompleteAsync(string lockToken);

      //  Task AbandonAsync(string lockToken);

      //  Task DeadLetterAsync(string lockToken);
        #endregion
    }
    #endregion
}