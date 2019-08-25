using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Aksl.AzureServiceBus
{
    #region IBusMessageDeadLetterReciever
    /// <summary>
    /// Bus Message Reciever Interface
    /// </summary>
    public interface IBusMessagePeekReciever
    {
        #region Properties
        /// <summary>
        /// Server Wait Time
        /// </summary>
        //TimeSpan ServerWaitTime
        //{
        //    get;
        //}
        IReceiverClient MessageReciever { get; }

        Action<MessageContext> OnRecieveCallBack { get; set; }
        #endregion

        #region Methods
        Task DeadLetterAsync(string lockToken, IDictionary<string, object> propertiesToModify = null);

        Task DeadLetterAsync(string lockToken, string deadLetterReason, string deadLetterErrorDescription = null);
        #endregion
    }
    #endregion
}