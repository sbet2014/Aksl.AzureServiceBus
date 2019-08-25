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
using Aksl.Concurrency;

namespace Aksl.AzureServiceBus
{
    /// <summary>
    /// Recieve ServiceBus Message
    /// </summary>
    public class AzureBusMessageReciever : IAzureBusMessageReciever  
    {
        #region Members
        public const double DefaultWaitTime = 5d;

        protected TimeSpan _serverWaitTime = TimeSpan.FromSeconds(DefaultWaitTime);

        /// <summary>
        /// Service Bus Message Client
        /// </summary>
        protected IMessageReceiver _messageReceiver = null;

        protected Action<MessageContext> _onRecieveCallBack;

        protected ILoggerFactory _loggerFactory;

        protected ILogger _logger;

        private readonly AsyncLock _mutex = new AsyncLock();
        #endregion

        #region Constructors
        public AzureBusMessageReciever(IMessageReceiver messageReceiver, double waitTime = DefaultWaitTime, Action<MessageContext> onRecieveCallBack = null, ILoggerFactory loggerFactory = null) =>
          InitializeBusMsmqReciever(messageReceiver, waitTime, onRecieveCallBack, loggerFactory);

        protected void InitializeBusMsmqReciever(IMessageReceiver messageReceiver, double waitTime, Action<MessageContext> onRecieveCallBack, ILoggerFactory loggerFactory)
        {
            _messageReceiver = messageReceiver;

            _serverWaitTime = TimeSpan.FromSeconds(waitTime <= 0 ? DefaultWaitTime : waitTime);
            _onRecieveCallBack = onRecieveCallBack;

            _loggerFactory = loggerFactory;
            _logger = loggerFactory?.CreateLogger(nameof(AzureBusMessageReciever));
        }
        #endregion

        #region Properties
        public TimeSpan ServerWaitTime => _serverWaitTime;

        public Action<MessageContext> OnRecieveCallBack
        {
            get => _onRecieveCallBack;
            set => _onRecieveCallBack = value;
        }
        #endregion

        #region Methods
        public async Task<Message> ReceiveAsync(TimeSpan operationTimeout, CancellationToken cancellationToken = default)
        {
            //using (await mutex.LockAsync(cancellationToken))
            //{
            var message = default(Message);
            var context = new MessageContext() { MessageConunt = 1 };

            var headBlock = default(BufferBlock<int>);
            var readBlock = default(TransformBlock<int, Message>);
            //  TimeSpan maxDuration = TimeSpan.Zero; ;//花去的最长时间

            while (true)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        //cancellationToken.ThrowIfCancellationRequested();
                        _logger?.LogCritical($"message recieve was cancelled");
                        break;
                    }

                    int maxDegreeOfParallelism = Environment.ProcessorCount * 1;//平行数
                    CreateBlockers(maxDegreeOfParallelism);

                    await headBlock.SendAsync(1)
                              .ConfigureAwait(continueOnCapturedContext: false);

                    //等待数据
                    var recieveMessage = await readBlock.ReceiveAsync()
                                             .ConfigureAwait(continueOnCapturedContext: false);

                    //context.ExecutionTime = maxDuration;
                    OnRecieveCallBack?.Invoke(context);

                    break;
                }
                catch (Exception ex)
                {
                    #region Methods
                    //_logger?.LogError($"Error when recieve message: '{ex.Message}'");

                    context.Exception = ex;
                    OnRecieveCallBack?.Invoke(context);
                    if (!context.Ignore)
                    {
                        throw;
                    }
                    #endregion
                }
            }

            void CreateBlockers(int maxDegreeOfParallelism)
            {
                #region Channel Methods
                headBlock = new BufferBlock<int>();

                readBlock = new TransformBlock<int, Message>(async (count) =>
                {
                    var sw = Stopwatch.StartNew();
                    var msg = await _messageReceiver.ReceiveAsync(operationTimeout);

                    //  context.ExecutionTime = context.ExecutionTime.Max(sw.Elapsed);

                    return msg;
                },
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    CancellationToken = cancellationToken
                });

                headBlock.LinkTo(readBlock);

                //await headBlock.SendAsync(1)
                //      .ConfigureAwait(continueOnCapturedContext: false);

                ////等待数据
                //var recieveMessage = await readBlock.ReceiveAsync()
                //                         .ConfigureAwait(continueOnCapturedContext: false);

                //_logger.LogInformation(
                //    $"---readed 1 message from msmq:\"{_currentQueue.QueueName}\"---duration: { maxDuration}---ThreadId:\"{Environment.CurrentManagedThreadId}\" ");
                //callBack(duration);

                //  return recieveMessage;
                #endregion
            }

            return message;
        }

        /// <summary>
        ///读多数据
        /// </summary>
        /// <param name="waitTime">队列等待时间</param>
        /// <param name="messageCount">消息数量</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IList<Message>> ReceiveBatchAsync(TimeSpan operationTimeout, int maxMessageCount = 10, CancellationToken cancellationToken = default)
        {
            if (maxMessageCount <= 0)
            {
                throw new ArgumentNullException("message count");
            }

            //using (await mutex.LockAsync(cancellationToken))
            //{
            var allMessages = default(List<Message>);

            var context = new MessageContext();

            var headBlock = default(BufferBlock<int>);
            var readActionBlocks = default(List<ActionBlock<int>>);
            TimeSpan maxExecutionTime = TimeSpan.Zero; ;//花去的最长时间

            using (await _mutex.LockAsync())//要读整个队列数量,这个方法会读取队列的数据，会改变队列数量
            {
                try
                {
                    var approixmateMessageCount = (int)await ApproixmateMessageCountAsync();
                    int currentMessageCount = Math.Min(approixmateMessageCount, maxMessageCount);
                    if (currentMessageCount <= 0)
                    {
                        return allMessages;
                    }

                    context.MessageConunt = currentMessageCount;
                    allMessages = new List<Message>(maxMessageCount);

                    #region Methods
                    int blockCount = Environment.ProcessorCount * 4;//块数
                    int minPerBlock = 8;//至少
                    int maxPerBlock = 200;//至多
                    int maxDegreeOfParallelism = Environment.ProcessorCount * 2;//并行数

                    int[] blockInfos = default(int[]);//分块
                    if (maxMessageCount < (blockCount * maxPerBlock))
                    {
                        blockInfos = BlockHelper.MacthBlockInfoDown(blockCount, maxMessageCount, minPerBlock);
                    }
                    else
                    {
                        blockInfos = BlockHelper.MacthBlockInfoUp(blockCount, maxMessageCount, maxPerBlock);
                    }

                    CreateBlockers(Environment.ProcessorCount, blockInfos);

                    for (int i = 0; i < blockInfos.Count(); i++)
                    {
                        await headBlock.SendAsync(blockInfos[i])
                                   .ConfigureAwait(continueOnCapturedContext: false);
                    }

                    //Parallel.ForEach(blockInfos, async info =>
                    //{
                    //    try
                    //    {
                    //        await headBlock.SendAsync(info)
                    //              .ConfigureAwait(continueOnCapturedContext: false);
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

                        foreach (var rab in readActionBlocks)
                        {
                            rab.Complete();
                            rab.Completion.Wait();
                        }
                    });

                    context.ExecutionTime = maxExecutionTime;
                    OnRecieveCallBack?.Invoke(context);
                    #endregion
                }
                catch (Exception ex)
                {
                    #region Methods
                    _logger?.LogError($"Error when recieve message: '{ex.Message}'");

                    context.Exception = ex;
                    OnRecieveCallBack?.Invoke(context);
                    if (!context.Ignore)
                    {
                        throw;
                    }
                    #endregion
                }

                void CreateBlockers(int maxDegreeOfParallelism,  IList<int> blockInfos)
                {
                    int boundedCapacity = blockInfos.Sum(b => b);

                    headBlock = new BufferBlock<int>();  //Producer
                    readActionBlocks = new List<ActionBlock<int>>(blockInfos.Count());//Consumers

                    //限制容量,做均衡负载
                    for (int i = 0; i < blockInfos.Count(); i++)
                    {
                        #region Methods
                        var readBlock = new ActionBlock<int>(async (count) =>
                        {
                            var sw = Stopwatch.StartNew();

                            List<Message> receivedMessages = new List<Message>(count);
                            List<string> lockTokens = new List<string>(count);

                            try
                            {
                                // var receivedMessages = await _messageReceiver.ReceiveAsync(count, operationTimeout);
                                for (int j = 0; j < count; j++)
                                {
                                    var receivedMessage = await _messageReceiver.ReceiveAsync(operationTimeout);
                                    receivedMessages.Add(receivedMessage);
                                    lockTokens.Add(receivedMessage.SystemProperties.LockToken);
                                }

                                if (receivedMessages.Count > 0)
                                {
                                    allMessages.AddRange(receivedMessages);
                                    await _messageReceiver.CompleteAsync(lockTokens);

                                    maxExecutionTime = maxExecutionTime.Ticks < sw.Elapsed.Ticks ? sw.Elapsed : maxExecutionTime;

                                    //_logger.LogInformation(
                                    //     $"---readed {pollMessages.Count} messages from msmq:\"{_currentQueue.QueueName}\"---duration: {pairsw.Elapsed}---ThreadId:\"{Environment.CurrentManagedThreadId}\" ");
                                }
                            }
                            catch (ServiceBusException sbex)
                            {
                               
                                context.Exception = sbex;
                            }
                        },
                        new ExecutionDataflowBlockOptions()
                        {
                            MaxDegreeOfParallelism = maxDegreeOfParallelism,
                            BoundedCapacity = blockInfos[i]
                        });

                        readActionBlocks.Add(readBlock);
                        #endregion
                    }

                    for (int i = 0; i < readActionBlocks.Count(); i++)
                    {
                        if (readActionBlocks[i] is ITargetBlock<int>)
                        {
                            headBlock.LinkTo(readActionBlocks[i], (count) =>
                            {
                                return count > 0;
                            });
                        }
                    }
                }

                return allMessages;
            }
            //}
        }
        #endregion

        #region  Prefetch Count
        public Task<int> ApproixmateMessageCountAsync()
        {
          var msgcount=(_messageReceiver as MessageReceiver).PrefetchCount;
            
            return Task.FromResult<int>(msgcount);
        }
        #endregion
    }
}