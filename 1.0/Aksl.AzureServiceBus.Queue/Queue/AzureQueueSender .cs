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
using Microsoft.Azure.ServiceBus.Primitives;
using Newtonsoft.Json;

//Install-Package Microsoft.Azure.ServiceBus -Pre 

namespace Aksl.AzureServiceBus.Queue
{
    public class AzureQueueSender : IAzureQueueSender
    {
        #region Members
        protected static QueueClient _client;

        protected ILoggerFactory _loggerFactory;

        protected Action<MessageContext> _onSendCallBack;

        protected ILogger _logger;

        protected readonly TimeSpan DefaultOperationTimeout = TimeSpan.FromMinutes(1);
        #endregion

        #region Constructors
        public AzureQueueSender(QueueClient client, ILoggerFactory loggerFactory = null) =>
                           InitializeAzureQueueSender(client,loggerFactory);

        protected void InitializeAzureQueueSender(QueueClient client, ILoggerFactory loggerFactory)
        {
            _client = client;

            _loggerFactory = loggerFactory;
            _logger = loggerFactory?.CreateLogger(nameof(AzureQueueSender));
        }
        #endregion

        #region Properties
        public Action<MessageContext> OnSendCallBack
        {
            get => _onSendCallBack;
            set => _onSendCallBack = value;
        }

        public QueueClient QueueClient
        {
            get => _client;
            set => _client = value ??
                         throw new ArgumentNullException(paramName: nameof(value), message: "queue client must not be null");
        }

        #endregion

        #region Helper Methods


        #endregion

        #region SendBatch Methods
        /// <summary>
        /// Send Batch Messages to Queue
        /// </summary>
        /// <param name="messages">Messages</param>
        /// <returns>Task</returns>
        public async Task SendBatchAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default)
        {
            if (messages == null)
            {
                throw new ArgumentNullException("messages is not null");
            }

            if (!messages.Any())
            {
                return;
            }

            if (messages.Any(m => m.Size > 256 * 1024))
            {
                throw new ArgumentNullException("have message must great than 256kb");
            }

            var headBlock = default(BufferBlock<Message[]>);
            var writeBlocks = default(List<ActionBlock<Message[]>>);

            int messageCount = messages.Count();
            var context = new MessageContext() { MessageConunt = messageCount };
            TimeSpan maxExecutionTime = TimeSpan.Zero; ;//花去的最长时间

            try
            {
                #region Block Methods
                int blockCount = Environment.ProcessorCount * 4;//块数
                int minPerBlock = 20;//至少有一块8条
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

                var blockMessages = BlockHelper.GetMessageByBlockInfo<Message>(blockInfos, messages.ToArray()).ToList();
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
                        wb.Completion.Wait(TimeSpan.FromSeconds(5));
                    }
                });

                context.ExecutionTime = maxExecutionTime;
                OnSendCallBack?.Invoke(context);
                #endregion
            }
            catch (Exception ex)
            {
                #region Methods
                _logger?.LogError($"when send message: '{ex.Message}'");

                context.Exception = ex;
                OnSendCallBack?.Invoke(context);
                if (!context.Ignore)
                {
                    throw;
                }
                #endregion
            }

            void CreateBlockers(int maxDegreeOfParallelism, int[] blockInfos, IList<Message[]> blockMessages)
            {
                #region Channel Methods
                headBlock = new BufferBlock<Message[]>();
                writeBlocks = new List<ActionBlock<Message[]>>(blockInfos.Count());

                //限制容量,做均衡负载
                for (int i = 0; i < blockInfos.Count(); i++)
                {
                    var writeBlock = new ActionBlock<Message[]>(async (blockmsgs) =>
                    {
                        var sw = Stopwatch.StartNew();

                        try
                        {
                            await _client.SendAsync(blockmsgs);

                            maxExecutionTime = maxExecutionTime.Ticks < sw.Elapsed.Ticks ? sw.Elapsed : maxExecutionTime;

                            _logger
                               .LogInformation($"ExecutionTime={sw.Elapsed},ThreadId={Thread.CurrentThread.ManagedThreadId},Count=\"{blockmsgs?.Count()}\"");

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
                    if (writeBlocks[i] is ITargetBlock<Message[]>)
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

        //public virtual async Task SendBatchAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default)
        //{
        //    if (null == messages)
        //    {
        //        throw new ArgumentNullException("messages");
        //    }

        //    if (messages.Any(m => m.Size > 256 * 1024))
        //    {
        //        throw new ArgumentNullException("have message must great than 256kb");
        //    }

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
        //            await _client.SendAsync(messages.ToList());

        //            context.ExecutionTime = sw.Elapsed;
        //            OnSendCallBack?.Invoke(context);

        //            break;
        //        }
        //        catch (Exception ex) when (ex is ServiceBusException sbex)
        //        {
        //            _logger?.LogError($"Error when send message: '{sbex.ToString()}'");

        //            context.Exception = sbex;
        //            OnSendCallBack?.Invoke(context);
        //            if (!context.Ignore)
        //            {
        //                throw;
        //            }

        //            if (sbex.IsTransient)
        //            {

        //            }

        //            break;
        //        }
        //    }
        //}
        #endregion

        #region  Close Methods
        public async Task CloseAsync()
        {
            await _client.CloseAsync();
        }
        #endregion
    }
}
