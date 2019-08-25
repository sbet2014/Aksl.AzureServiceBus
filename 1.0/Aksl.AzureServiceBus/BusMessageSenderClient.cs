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

namespace Aksl.AzureServiceBus
{
    /// <summary>
    /// BusMessage Sender Client
    /// </summary>
    public class BusMessageSenderClient : IBusMessageSenderClient
    {
        #region Members
        /// <summary>
        /// Service Bus Message Client
        /// </summary>
        protected ISenderClient _messageSender = null;

        protected ILoggerFactory _loggerFactory;

        protected ILogger _logger;

       protected Action<MessageContext> _onSendCallBack;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageSender"></param>
        /// <param name="logger"></param>
        public BusMessageSenderClient(ISenderClient messageSender, ILoggerFactory loggerFactory = null)=>
             InitializeBusMessageSender( messageSender, loggerFactory );

        protected void InitializeBusMessageSender(ISenderClient messageSender, ILoggerFactory loggerFactory)
        {
            MessageSender = messageSender;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory?.CreateLogger(nameof(BusMessageSenderClient));
        }
        #endregion

        #region Properties
        public virtual ISenderClient MessageSender
        {
            get => _messageSender;
            set => _messageSender = value ??
                       throw new ArgumentNullException(paramName: nameof(value), message: "message sender must not be null");
        }

        public Action<MessageContext> OnSendCallBack
        {
            get => _onSendCallBack;
            set => _onSendCallBack = value;
        }
        #endregion

        #region Send Methods
        /// <summary>
        /// Send Message to Queue
        /// </summary>
        /// <param name="message">Message,最大为256KB ,标头最大为64KB</param>
        /// <returns>Task</returns>
        public async Task SendAsync(Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (null == message)
            {
                throw new ArgumentNullException("message");
            }
            if (message.Size > 256 * 1024)
            {
                throw new ArgumentNullException("message must less than 256kb");
            }

            var context = new MessageContext() { MessageConunt = 1 };
            var sw = Stopwatch.StartNew();

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await _messageSender.SendAsync(message);

                    context.ExecutionTime = sw.Elapsed;
                    OnSendCallBack?.Invoke(context);

                    break;
                }
                catch (MessageLockLostException mex)
                {
                    _logger?.LogError($"Error when send message: '{mex.ToString()}'");

                    context.Exception = mex;
                    OnSendCallBack?.Invoke(context);
                    if (!context.Ignore)
                    {
                        throw;
                    }

                    if (mex.IsTransient)
                    {
                      
                    }
                }
            }
        }

        /// <summary>
        /// Save Object to queue, as json
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="encoding">Encoding (Default Json)</param>
        /// <returns>Task</returns>
        public async Task SendAsync(object obj,  CancellationToken cancellationToken = default(CancellationToken))
        {
            if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }

            if (obj is Message msg)
            {
                await this.SendAsync(msg, cancellationToken);
            }
            else
            {
                var brokeredMessage = new Message(Encoding.UTF8.GetBytes(obj.ToString()))
                {
                    ContentType = obj.GetType().ToString(),
                };

                await this.SendAsync(brokeredMessage, cancellationToken);
            }
        }
        #endregion

        #region SendBatch Methods
        /// <summary>
        /// Send Message to Queue
        /// </summary>
        /// <param name="messages">Messages</param>
        /// <returns>Task</returns>
        public virtual async Task SendBatchAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (null == messages)
            {
                throw new ArgumentNullException("messages");
            }

            if ( messages.Any(m=> m.Size > 256 * 1024))
            {
                throw new ArgumentNullException("have message must great than 256kb");
            }

            var context = new MessageContext() { MessageConunt = messages.Count() };
            var sw = Stopwatch.StartNew();

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await _messageSender.SendAsync(messages.ToList());

                    context.ExecutionTime = sw.Elapsed;
                    OnSendCallBack?.Invoke(context);

                    break;
                }
                catch (Exception ex) when (ex is ServiceBusException sbex)
                {
                    _logger?.LogError($"Error when send message: '{sbex.ToString()}'");

                    context.Exception = sbex;
                    OnSendCallBack?.Invoke(context);
                    if (!context.Ignore)
                    {
                        throw;
                    }

                    if (sbex.IsTransient)
                    {

                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Send Object to queue, as json
        /// </summary>
        /// <param name="messages">Messages</param>
        /// <param name="encoding">Encoding (Default Json)</param>
        /// <returns>Task</returns>
        public virtual async Task SendBatchAsync(IEnumerable<object> messages,  CancellationToken cancellationToken = default(CancellationToken))
        {
            if (null == messages)
            {
                throw new ArgumentNullException("obj");
            }

            if (messages is IEnumerable<Message> msgs)
            {
                await this.SendBatchAsync(msgs, cancellationToken);
            }
            else
            {
                var brokeredMessages = new List<Message>(messages.Count());
                foreach (var m in messages)
                {
                    var data = Encoding.UTF8.GetBytes( JsonConvert.SerializeObject(m)) ;
                    var brokeredMessage = new Message(data)
                    {
                        ContentType = m.GetType().ToString(),
                    };
                }

                await this.SendBatchAsync(brokeredMessages, cancellationToken);
            }
        }
        #endregion

        #region  Schedule Methods
        public async Task<long> ScheduleMessageAsync(Message message, DateTimeOffset scheduleEnqueueTimeUtc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (null == message)
            {
                throw new ArgumentNullException("message");
            }

            if (message.Size > 256 * 1024)
            {
                throw new ArgumentNullException("have message must great than 256kb");
            }
         
            var context = new MessageContext() { MessageConunt =1 };
            var sw = Stopwatch.StartNew();
            var result = default(long);

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                   return result;
                }

                try
                {
                  result=    await _messageSender.ScheduleMessageAsync(message, scheduleEnqueueTimeUtc);

                    context.ExecutionTime = sw.Elapsed;
                    OnSendCallBack?.Invoke(context);

                    return result;
                }
                catch (Exception ex) when (ex is ServiceBusException sbex)
                {
                    _logger?.LogError($"Error when send message: '{sbex.ToString()}'");

                    context.Exception = sbex;
                    OnSendCallBack?.Invoke(context);
                    if (!context.Ignore)
                    {
                        throw;
                    }

                    if (sbex.IsTransient)
                    {

                    }
                }
            }
        }

        public async Task CancelScheduledMessageAsync(long sequenceNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            if ( sequenceNumber< long.MinValue)
            {
                throw new ArgumentNullException("sequenceNumber");
            }

            var context = new MessageContext() { MessageConunt = 1 };
            var sw = Stopwatch.StartNew();

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                   break;
                }

                try
                {
                   await _messageSender.CancelScheduledMessageAsync(sequenceNumber);

                    context.ExecutionTime = sw.Elapsed;
                    OnSendCallBack?.Invoke(context);

                    break;
                }
                catch (Exception ex) when (ex is ServiceBusException sbex)
                {
                    _logger?.LogError($"Error when send message: '{sbex.ToString()}'");

                    context.Exception = sbex;
                    OnSendCallBack?.Invoke(context);
                    if (!context.Ignore)
                    {
                        throw;
                    }

                    if (sbex.IsTransient)
                    {

                    }
                }
            }
        }
        #endregion

        #region  Close Methods
        public async Task CloseAsync()
        {
           await  _messageSender.CloseAsync();
        }
        #endregion
    }
}