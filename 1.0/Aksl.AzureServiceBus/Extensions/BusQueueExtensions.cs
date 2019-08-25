//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Threading.Tasks.Dataflow;

//using Microsoft.Extensions.Logging;
//using Microsoft.Azure.ServiceBus;
//using Microsoft.Azure.ServiceBus.Core;
//using Newtonsoft.Json;

//namespace Aksl.AzureServiceBus
//{
//    public static class AzureQueueExtensions
//    {
//        public static IAzureQueueService CreateAzureQueueServiceAsync(this (ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, ReceiveMode mode, double operationTimeout, RetryPolicy retryPolicy, ILoggerFactory loggerFactory) createInfo)
//        {
//            try
//            {
//                var busQueue = new AzureQueueService(createInfo.serviceBusConnectionStringBuilder, createInfo.mode, createInfo.operationTimeout, createInfo.retryPolicy, createInfo.loggerFactory);
//                return busQueue;
//            }
//            catch { }

//            return null;
//        }

//        public static IAzureQueueSender CreateBusMessageSenderAsync(this (ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, ReceiveMode mode, double operationTimeout, RetryPolicy retryPolicy, ILoggerFactory loggerFactory) createInfo)
//        {
//            try
//            {
//                var  queueService = new AzureQueueService(createInfo.serviceBusConnectionStringBuilder, createInfo.mode, createInfo.operationTimeout, createInfo.retryPolicy, createInfo.loggerFactory);

//                var sender = queueService.CreateQueueSender();
//                return sender;
//            }
//            catch { }

//            return null;
//        }

//        public static async Task ExcuteSendAsync(this IBusQueue busQueue, Action<IBusMessageSender, CancellationToken> action, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            var initializeBlock = new TransformBlock<IBusQueue, IBusMessageSender>((bq) =>
//           {
//               try
//               {
//                   var sender = bq.CreateMessageSender();
//                   return sender;
//               }
//               catch { }

//               return null;
//           },
//             new ExecutionDataflowBlockOptions()
//             {
//                 CancellationToken = cancellationToken
//             });

//            var sendBlock = new ActionBlock<IBusMessageSender>((sender) =>
//           {
//               action(sender, cancellationToken);
//           },
//            new ExecutionDataflowBlockOptions()
//            {
//                CancellationToken = cancellationToken
//            });

//            initializeBlock.LinkTo(sendBlock, (sender) =>
//            {
//                return sender != null;
//            });

//            await initializeBlock.SendAsync(busQueue)
//                       .ConfigureAwait(continueOnCapturedContext: false);
//        }

//        public static async Task ExcuteRecieveAsync(this IBusQueue busQueue, Action<IBusMessageReciever, CancellationToken> action, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            var initializeBlock = new TransformBlock<IBusQueue, IBusMessageReciever>((bh) =>
//            {
//                try
//                {
//                    var reciever = busQueue.CreateMessageReceiver();
//                    return reciever;
//                }
//                catch { }

//                return null;
//            },
//             new ExecutionDataflowBlockOptions()
//             {
//                 CancellationToken = cancellationToken
//             });

//            var recieveBlock = new ActionBlock<IBusMessageReciever>((reciever) =>
//            {
//                try
//                {
//                    action(reciever, cancellationToken);
//                }
//                catch (Exception ex)
//                {
//                    throw ex;
//                }
//            },
//            new ExecutionDataflowBlockOptions()
//            {
//                CancellationToken = cancellationToken
//            });

//            initializeBlock.LinkTo(recieveBlock, (reciever) =>
//            {
//                return reciever != null;
//            });

//            await initializeBlock.SendAsync(busQueue)
//                       .ConfigureAwait(continueOnCapturedContext: false);
//        }
//    }
//}
