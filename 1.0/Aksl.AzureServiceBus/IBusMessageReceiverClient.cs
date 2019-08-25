using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Aksl.AzureServiceBus
{
    #region IBusMessageReciever
    /// <summary>
    /// Bus Message Reciever Interface
    /// </summary>
    public interface IBusMessageReceiverClient
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
        void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler);

        void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, MessageHandlerOptions messageHandlerOptions);

        void RegisterSessionHandler(Func<IMessageSession, Message, CancellationToken, Task> handler, Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler);

        void RegisterSessionHandler(Func<IMessageSession, Message, CancellationToken, Task> handler, SessionHandlerOptions sessionHandlerOptions);

        Task CompleteAsync(string lockToken);

        Task AbandonAsync(string lockToken);

        Task DeadLetterAsync(string lockToken);
        #endregion
    }
    #endregion
}