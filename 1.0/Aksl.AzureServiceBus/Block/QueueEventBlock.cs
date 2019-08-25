using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Aksl.AzureServiceBus
{
    public class QueueEventBlock<T>  
    {
        #region Members
        protected IQueueClient _reciever;

        protected ActionBlock<Message> _sourceBlocker;

        protected BufferBlock<Message> _targetBlocker;

        protected Action<T> _action;

        protected Func<ExceptionReceivedEventArgs, Task> _onError;

        protected CancellationToken _cancellationToken;

        protected ILoggerFactory _loggerFactory;

        protected ILogger _logger;

        protected MessageHandlerOptions _messageHandlerOptions;

        //protected MessageHandlerOptions DefaultMessageHandlerOptions => 
        //      new MessageHandlerOptions() { AutoComplete = true, MaxConcurrentCalls = 1 , MaxAutoRenewDuration= TimeSpan.FromMinutes(5) };
        #endregion

        #region Constructors
        //public QueueEventBlock(IMessageReceiver reciever, Action<T> onAction, Func<ExceptionReceivedEventArgs, Task>  onError, Action<T> onCompleted, /*MessageHandlerOptions messageHandlerOptions = null,*/ CancellationToken cancellationToken = default(CancellationToken), ILoggerFactory loggerFactory = null) =>
        //    InitializeEventBlock( reciever, onAction, onError, /*messageHandlerOptions,*/  cancellationToken,  loggerFactory);

        public QueueEventBlock(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder,
                             Action<T> onAction, Func<ExceptionReceivedEventArgs, Task> onError, Action<T> onCompleted, ReceiveMode mode = ReceiveMode.PeekLock, double operationTimeout = 1, RetryPolicy retryPolicy = null,
                             CancellationToken cancellationToken = default, ILoggerFactory loggerFactory = null) =>
          InitializeEventBlock(serviceBusConnectionStringBuilder, mode, retryPolicy, onAction, onError, /*messageHandlerOptions,*/  cancellationToken, loggerFactory);

        protected void InitializeEventBlock(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, ReceiveMode mode, RetryPolicy retryPolicy, Action<T> action, Func<ExceptionReceivedEventArgs, Task> onError, CancellationToken cancellationToken, ILoggerFactory loggerFactory)
        {
            _reciever = CreateMessageSender(serviceBusConnectionStringBuilder, mode, retryPolicy);
            //   _messageHandlerOptions = messageHandlerOptions ?? DefaultMessageHandlerOptions;
            _cancellationToken = cancellationToken;
            _action = action;

            _messageHandlerOptions = new MessageHandlerOptions(onError);

            _logger = loggerFactory?.CreateLogger(nameof(QueueEventBlock<T>));
        }

        protected IQueueClient CreateMessageSender(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, ReceiveMode mode, RetryPolicy retryPolicy)
        {
            _logger.LogInformation($"creating a new message receiver");
            var reciever = new QueueClient(serviceBusConnectionStringBuilder, mode, retryPolicy);
            return reciever;
        }
        #endregion

        #region Properties
        public IQueueClient Reciever
        {
            get => _reciever;
            private set => _reciever = value ??
                   throw new ArgumentNullException(paramName: nameof(value), message: "message reciever must not be null");
        }

        public MessageHandlerOptions MessageHandlerOptions => _messageHandlerOptions;
        #endregion

        #region Methods
        public async Task<bool> RunAsync()
        {
            var workDone = false;

            try
            {
                //注册异常事件
                 //this.MessageHandlerOptions.ExceptionReceivedHandler = OnExceptionReceived;

                //注册接收事件
                this.Reciever.RegisterMessageHandler(OnMessageArrived, this.MessageHandlerOptions);

                workDone = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Error while poll messsage: {0}", ex.Message);
            }

            return await Task.FromResult(workDone);
        }

        protected virtual void OnExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            if (e != null && e.Exception != null)
            {
                //唤醒其它进程
                // this.asyncManualResetEvent.Set();
                //  _logger?.LogError("'{0}' {1}", e.Action, e.Exception.ToString());
            }
        }

        protected  async Task OnMessageArrived(Message message, CancellationToken cancellationToken)
        {
            #region Task
            var task = Task.Run(async () =>
            {
                try
                {
                    //阻塞其它进程
                    //  this.asyncManualResetEvent.Reset();

                    if (null != message)
                    {
                        CreateBlockers();

                        if (_targetBlocker is ITargetBlock<Message> messageSender)
                        {
                            await messageSender.SendAsync(message);

                            messageSender.Complete();
                            await messageSender.Completion.ContinueWith(_ =>
                            {
                                _logger.LogInformation($"{1} message was send by {messageSender.GetType()}.");
                            });
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No body.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(0, ex, "Error while on message arrived: {0}", ex.Message);
                }
                finally
                {
                    //唤醒其它进程
                    //  this.asyncManualResetEvent.Set();
                }
            });
            #endregion

            await task;
        }

        protected void CreateBlockers()
        {
            //Consumer,处理数据
            _sourceBlocker = new ActionBlock<Message>(async (message) =>
            {
                var data = await ProcessMessageAsync(message);

                _action?.Invoke(data);
            },
            new ExecutionDataflowBlockOptions()
            {
                CancellationToken = _cancellationToken
            });

            //Producer
            _targetBlocker = new BufferBlock<Message>(
            new ExecutionDataflowBlockOptions()
            {
                CancellationToken = _cancellationToken
            });

            _targetBlocker.LinkTo(_sourceBlocker as ITargetBlock<Message>);
        }

        protected async Task<T> ProcessMessageAsync(Message message)
        {
            T data = default(T);
            try
            {
                data = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(message.Body));
                if (null != data)
                {
                    await this.Reciever.CompleteAsync(message.SystemProperties.LockToken);

                    _logger?.LogInformation($"Process Message Complete.");
                }
                return data;
            }
            catch (Exception ex)
            {
                await this.Reciever.AbandonAsync(message.SystemProperties.LockToken);

                _logger?.LogError(0, ex, "Error while process messsage body: {0}", ex.Message);
            }
            return data;
        }

        //public async Task<bool> StopAsync()
        //{

        //    _cancellationToken.WaitHandle;
        //    return true;
        //}

        #endregion
    }
}
