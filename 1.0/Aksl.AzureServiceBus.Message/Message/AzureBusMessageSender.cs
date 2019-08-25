using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace Aksl.AzureServiceBus.Message
{
    /// <summary>
    /// Azure Bus Message Sender
    /// </summary>
    public class AzureBusMessageSender : IAzureBusMessageSender
    {
        #region Members
        /// <summary>
        /// Service Bus Message Client
        /// </summary>
        protected IMessageSender _messageSender = null;

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
        public AzureBusMessageSender(IMessageSender messageSender, ILoggerFactory loggerFactory = null)=>
             InitializeBusMessageSender( messageSender, loggerFactory );

        protected void InitializeBusMessageSender(IMessageSender messageSender, ILoggerFactory loggerFactory)
        {
            MessageSender = messageSender;

            _loggerFactory = loggerFactory;
            _logger = loggerFactory?.CreateLogger(nameof(AzureBusMessageSender));
        }
        #endregion

        #region Properties
        public  IMessageSender MessageSender
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
        public async Task SendAsync(Microsoft.Azure.ServiceBus.Message message, CancellationToken cancellationToken = default)
        {
            if (null == message)
            {
                throw new ArgumentNullException("message");
            }
            if (message.Size > 256 * 1024)
            {
                throw new ArgumentNullException("message must less than 256kb");
            }

            var headBlock = default(BufferBlock<Microsoft.Azure.ServiceBus.Message>);
            var writeBlock = default(ActionBlock<Microsoft.Azure.ServiceBus.Message>);

            var context = new MessageContext() { MessageConunt = 1 };

            try
            {
                int maxDegreeOfParallelism = Environment.ProcessorCount * 1;//平行数
                CreateBlockers(maxDegreeOfParallelism);

                await headBlock.SendAsync(message)
                         .ConfigureAwait(continueOnCapturedContext: false);

                headBlock.Complete();
                await headBlock.Completion.ContinueWith(_ =>
                {
                    //  _logger.LogInformation($"{1} message was send by {headBlock.GetType()}.");

                    writeBlock.Complete();
                    writeBlock.Completion.Wait();
                });

                OnSendCallBack?.Invoke(context);
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
            }

            void CreateBlockers(int maxDegreeOfParallelism)
            {
                #region Methods
                //Producer
                headBlock = new BufferBlock<Microsoft.Azure.ServiceBus.Message>();

                //Consumer
                writeBlock = new ActionBlock<Microsoft.Azure.ServiceBus.Message>(async (msg) =>
                {
                    var sw = Stopwatch.StartNew();

                    try
                    {
                        await _messageSender.SendAsync(message);

                        //_logger.LogInformation(
                        //    $"--sended 1 message :\"{_messageSender.ClientId}\"--duration: {sw.Elapsed}--Now:\"{DateTime.Now.TimeOfDay}\"--ThreadId:\"{Environment.CurrentManagedThreadId}\" ");

                        context.ExecutionTime = sw.Elapsed;
                    }
                    catch (Exception ex)
                    {
                        context.Exception = ex;
                    }
                },
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    CancellationToken = cancellationToken
                });

                headBlock.LinkTo(writeBlock);
                #endregion
            }
        }

        /// <summary>
        /// Save Object to queue, as json
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="encoding">Encoding (Default Json)</param>
        /// <returns>Task</returns>
        public async Task SendAsync(object obj,  CancellationToken cancellationToken = default)
        {
            if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }

            if (obj is Microsoft.Azure.ServiceBus.Message msg)
            {
                await this.SendAsync(msg, cancellationToken);
            }
            else
            {
                var brokeredMessage = new Microsoft.Azure.ServiceBus.Message(Encoding.UTF8.GetBytes(obj.ToString()))
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
        public virtual async Task SendBatchAsync(IEnumerable<Microsoft.Azure.ServiceBus.Message> messages, CancellationToken cancellationToken = default)
        {
            if (null == messages)
            {
                throw new ArgumentNullException("messages");
            }

            if (!messages.Any())
            {
                return;
            }

            if (messages.Any(m => m.Size > 256 * 1024))
            {
                throw new ArgumentNullException("have message must great than 256kb");
            }

            var headBlock = default(BufferBlock<Microsoft.Azure.ServiceBus.Message[]>);
            var writeBlocks = default(List<ActionBlock<Microsoft.Azure.ServiceBus.Message[]>>);

            int messageCount = messages.Count();
            var context = new MessageContext() { MessageConunt = messages.Count() };
            TimeSpan maxDurationlism = TimeSpan.Zero; ;//花去的最长时间

            try
            {
                #region Block Methods
                int blockCount = Environment.ProcessorCount * 4;//块数
                int minPerBlock = 8;//至少有一块8条
                int maxPerBlock = 200;//至多
                int maxDegreeOfParallelism = Environment.ProcessorCount * 2;//并行数

                //分块
                //var blockInfos = BlockHelper.MacthBlockInfoDown(blockCount, messageCount, minPerBlock);
                int[] blockInfos = default;
                if (messageCount < (blockCount * maxPerBlock))
                {
                    //分块
                    blockInfos = BlockHelper.MacthBlockInfoDown(blockCount, messageCount, minPerBlock);
                }
                else
                {
                    blockInfos = BlockHelper.MacthBlockInfoUp(blockCount, messageCount, maxPerBlock);
                }

                var blockMessages = BlockHelper.GetMessageByBlockInfo<Microsoft.Azure.ServiceBus.Message>(blockInfos, messages.ToArray()).ToList();
                #endregion

                #region Send Methods
                //创建通道
                CreateBlockers(maxDegreeOfParallelism, blockInfos, blockMessages);

                foreach (var msg in blockMessages)
                {
                    await headBlock.SendAsync(msg)
                       .ConfigureAwait(continueOnCapturedContext: false);
                }

                //Parallel.ForEach(blockMessages, async msg =>
                //{
                //    try
                //    {
                //        await headBlock.SendAsync(msg);
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogError($"exception: {ex.Message} when block send message");
                //    }
                //});

                headBlock.Complete();
                await headBlock.Completion.ContinueWith(_ =>
                {
                    //  _logger.LogInformation($"{1} message was send by {headBlock.GetType()}.");

                    foreach (var wb in writeBlocks)
                    {
                        wb.Complete();
                        wb.Completion.Wait();
                    }
                });

                context.ExecutionTime = maxDurationlism;
                OnSendCallBack?.Invoke(context);
                #endregion

            }
            catch (Exception ex) when (ex is ServiceBusException sbex)
            {
                #region Methods
                _logger?.LogError($"Error when send message: '{sbex.ToString()}'");

                context.Exception = sbex;
                OnSendCallBack?.Invoke(context);
                if (!context.Ignore)
                {
                    throw;
                }
                #endregion
            }

            void CreateBlockers(int maxDegreeOfParallelism, int[] blockInfos, IList<Microsoft.Azure.ServiceBus.Message[]> blockMessages)
            {
                #region Channel Methods
                headBlock = new BufferBlock<Microsoft.Azure.ServiceBus.Message[]>();
                writeBlocks = new List<ActionBlock<Microsoft.Azure.ServiceBus.Message[]>>(blockInfos.Count());

                //限制容量,做均衡负载
                for (int i = 0; i < blockInfos.Count(); i++)
                {
                    var writeBlock = new ActionBlock<Microsoft.Azure.ServiceBus.Message[]>(async (blockmsgs) =>
                    {
                        var sw = Stopwatch.StartNew();

                        try
                        {
                            await _messageSender.SendAsync(messages.ToList());

                            _logger.LogInformation(
                                   $"--sended {blockmsgs.Count()} message to msmq:\"{_messageSender.ClientId}\"--duration: {sw.Elapsed}--Now:\"{DateTime.Now.TimeOfDay}\"--ThreadId:\"{Environment.CurrentManagedThreadId}\" ");

                            maxDurationlism = maxDurationlism.Ticks < sw.Elapsed.Ticks ? sw.Elapsed : maxDurationlism;
                        }
                        catch (Exception ex)
                        {
                            context.Exception = ex;
                        }
                    },
                    new ExecutionDataflowBlockOptions()
                    {
                        BoundedCapacity = blockInfos[i],//限制容量,做均衡负载
                        CancellationToken = cancellationToken
                    });

                    writeBlocks.Add(writeBlock);
                }

                for (int i = 0; i < writeBlocks.Count(); i++)
                {
                    if (writeBlocks[i] is ITargetBlock<Microsoft.Azure.ServiceBus.Message[]>)
                    {
                        headBlock.LinkTo(writeBlocks[i], (msgs) =>
                        {
                            return msgs?.Count() > 0;
                        });
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Send Object to queue, as json
        /// </summary>
        /// <param name="messages">Messages</param>
        /// <param name="encoding">Encoding (Default Json)</param>
        /// <returns>Task</returns>
        public virtual async Task SendBatchAsync(IEnumerable<object> messages,  CancellationToken cancellationToken = default)
        {
            if (null == messages)
            {
                throw new ArgumentNullException("obj");
            }

            if (messages is IEnumerable<Microsoft.Azure.ServiceBus.Message> msgs)
            {
                await this.SendBatchAsync(msgs, cancellationToken);
            }
            else
            {
                var brokeredMessages = new List<Microsoft.Azure.ServiceBus.Message>(messages.Count());
                foreach (var m in messages)
                {
                    var data = Encoding.UTF8.GetBytes( JsonConvert.SerializeObject(m)) ;
                    var brokeredMessage = new Microsoft.Azure.ServiceBus.Message(data)
                    {
                        ContentType = m.GetType().ToString(),
                    };
                }

                await this.SendBatchAsync(brokeredMessages, cancellationToken);
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