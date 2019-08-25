using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

using Aksl.Concurrency;

namespace Aksl.AzureServiceBus
{
    /// <summary>
    /// Bus Message Reciever
    /// </summary>
    public class BusMessageReceiverClient : IBusMessageReceiverClient
    {
        #region Members
        protected IReceiverClient _messageReciever;

        protected Action<MessageContext> _onRecieveCallBack;

        protected ILoggerFactory _loggerFactory;

        protected ILogger _logger;

        /// <summary>
        /// Server Wait Time in Seconds
        /// </summary>
        public const int DefaultWaitTime = 15;

        /// <summary>
        /// Server Wait Time
        /// </summary>
        protected TimeSpan _serverWaitTime = TimeSpan.FromSeconds(DefaultWaitTime);

        //private readonly AsyncLock _mutex = new AsyncLock();
        #endregion

        #region Constructors
        public BusMessageReceiverClient(IReceiverClient messageReciever, ILoggerFactory loggerFactory = null) =>
                                    InitializeReciever(messageReciever, loggerFactory);

        protected void InitializeReciever(IReceiverClient messageReciever, ILoggerFactory loggerFactory)
        {
            MessageReciever = messageReciever;

            //  _serverWaitTime = TimeSpan.FromSeconds(waitTime <= 0 ? DefaultWaitTime : waitTime);

            _loggerFactory = loggerFactory;
            _logger = loggerFactory?.CreateLogger(nameof(BusMessageReceiverClient));
        }
        #endregion

        #region Properties
        public IReceiverClient MessageReciever
        {
            get => _messageReciever;
            set => _messageReciever = value ??
                       throw new ArgumentNullException("message reciever");
        }

        //     public TimeSpan ServerWaitTime => _serverWaitTime;

        public Action<MessageContext> OnRecieveCallBack
        {
            get => _onRecieveCallBack;
            set => _onRecieveCallBack = value;
        }

        #endregion

        #region IBusMessageReciever
        //public async  Task<Message> ReceiveAsync(TimeSpan waitTime, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var message = default(Message);
        //    var context = new MessageContext() { MessageConunt = 1 };
        //    var sw = Stopwatch.StartNew();
        //    waitTime = waitTime <= TimeSpan.Zero ? _serverWaitTime : waitTime;

        //    while (true)
        //    {
        //        if (cancellationToken.IsCancellationRequested)
        //        {
        //            break;
        //        }

        //        try
        //        {
        //            message = await _messageReciever.ReceiveAsync(waitTime);

        //            context.ExecutionTime = sw.Elapsed;
        //            OnRecieveCallBack?.Invoke(context);

        //            break;
        //        }
        //        catch (MessageLockLostException mex)
        //        {
        //            _logger?.LogError($"Error when send message: '{mex.ToString()}'");

        //            context.Exception = mex;
        //            OnRecieveCallBack?.Invoke(context);
        //            if (!context.Ignore)
        //            {
        //                throw;
        //            }

        //            if (mex.IsTransient)
        //            {

        //            }
        //        }
        //    }

        //    return message;
        //}

        //public async Task<IList<Message>> ReceiveBatchAsync(TimeSpan waitTime, int maxMessageCount = 10, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var messages = default(IList<Message>);

        //    var context = new MessageContext() { MessageConunt = messages.Count()};
        //    var sw = Stopwatch.StartNew();
        //    waitTime = waitTime <= TimeSpan.Zero ? _serverWaitTime : waitTime;

        //    while (true)
        //    {
        //        if (cancellationToken.IsCancellationRequested)
        //        {
        //            break;
        //        }

        //        try
        //        {
        //            messages = await _messageReciever.ReceiveAsync(maxMessageCount, waitTime);

        //            context.ExecutionTime = sw.Elapsed;
        //            OnRecieveCallBack?.Invoke(context);

        //            break;
        //        }
        //        catch (MessageLockLostException mex)
        //        {
        //            _logger?.LogError($"Error when send message: '{mex.ToString()}'");

        //            context.Exception = mex;
        //            OnRecieveCallBack?.Invoke(context);
        //            if (!context.Ignore)
        //            {
        //                throw;
        //            }

        //            if (mex.IsTransient)
        //            {

        //            }
        //        }
        //    }

        //    return messages;
        //}

        //public async Task<Message> ReceiveBySequenceNumberAsync(long sequenceNumber, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var message = default(Message);
        //    var context = new MessageContext() { MessageConunt = 1 };
        //    var sw = Stopwatch.StartNew();

        //    while (true)
        //    {
        //        if (cancellationToken.IsCancellationRequested)
        //        {
        //            break;
        //        }

        //        try
        //        {
        //            message = await _messageReciever.ReceiveBySequenceNumberAsync(sequenceNumber);

        //            context.ExecutionTime = sw.Elapsed;
        //            OnRecieveCallBack?.Invoke(context);

        //            break;
        //        }
        //        catch (MessageLockLostException mex)
        //        {
        //            _logger?.LogError($"Error when send message: '{mex.ToString()}'");

        //            context.Exception = mex;
        //            OnRecieveCallBack?.Invoke(context);
        //            if (!context.Ignore)
        //            {
        //                throw;
        //            }

        //            if (mex.IsTransient)
        //            {

        //            }
        //        }
        //    }

        //    return message;
        //}

        //public async Task<IList<Message>> ReceiveBySequenceNumberAsync(IEnumerable<long> sequenceNumbers, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var messages = default(IList<Message>);

        //    var context = new MessageContext() { MessageConunt = messages.Count() };
        //    var sw = Stopwatch.StartNew();

        //    while (true)
        //    {
        //        if (cancellationToken.IsCancellationRequested)
        //        {
        //            break;
        //        }

        //        try
        //        {
        //            messages = await _messageReciever.ReceiveBySequenceNumberAsync(sequenceNumbers);

        //            context.ExecutionTime = sw.Elapsed;
        //            OnRecieveCallBack?.Invoke(context);

        //            break;
        //        }
        //        catch (MessageLockLostException mex)
        //        {
        //            _logger?.LogError($"Error when send message: '{mex.ToString()}'");

        //            context.Exception = mex;
        //            OnRecieveCallBack?.Invoke(context);
        //            if (!context.Ignore)
        //            {
        //                throw;
        //            }

        //            if (mex.IsTransient)
        //            {

        //            }
        //        }
        //    }

        //    return messages;
        //}
        #endregion

        #region IBusMessageReciever
        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler)
        {
            if (null == exceptionReceivedHandler)
            {
                throw new ArgumentNullException("exception received handler");
            }

            _messageReciever.RegisterMessageHandler(handler, exceptionReceivedHandler);
        }

        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, MessageHandlerOptions registerHandlerOptions)
        {
            if (null == handler)
            {
                throw new ArgumentNullException("handler");
            }

            if (null == registerHandlerOptions)
            {
                throw new ArgumentNullException("registerhandler options");
            }

            _messageReciever.RegisterMessageHandler(handler, registerHandlerOptions);
        }

        public void RegisterSessionHandler(Func<IMessageSession, Message, CancellationToken, Task> handler, Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler)
        {
            if (null == handler)
            {
                throw new ArgumentNullException("handler");
            }

            if (_messageReciever is QueueClient qc)
            {
                qc.RegisterSessionHandler(handler, exceptionReceivedHandler);
            }

            if (_messageReciever is SubscriptionClient sc)
            {
                sc.RegisterSessionHandler(handler, exceptionReceivedHandler);
            }
        }

        public void RegisterSessionHandler(Func<IMessageSession, Message, CancellationToken, Task> handler, SessionHandlerOptions sessionHandlerOptions)
        {
            if (null == handler)
            {
                throw new ArgumentNullException("handler");
            }

            if (null == sessionHandlerOptions)
            {
                throw new ArgumentNullException("sessionhandler options");
            }

            if (_messageReciever is QueueClient qc)
            {
                qc.RegisterSessionHandler(handler, sessionHandlerOptions);
            }

            if (_messageReciever is SubscriptionClient sc)
            {
                sc.RegisterSessionHandler(handler, sessionHandlerOptions);
            }
        }

        public async Task CompleteAsync(string lockToken)
        {
            await _messageReciever.CompleteAsync(lockToken);
        }

        public async Task AbandonAsync(string lockToken)
        {
            await _messageReciever.AbandonAsync(lockToken);
        }

        public async Task DeadLetterAsync(string lockToken)
        {
            await _messageReciever.DeadLetterAsync(lockToken);
        }
        #endregion
    }
}