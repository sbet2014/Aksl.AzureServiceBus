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
//    public static class BusQueueSenderExtensions
//    {
//        public static IBusQueueSender CreateBusQueueSender(this (ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, ReceiveMode mode, double operationTimeout, RetryPolicy retryPolicy, ILoggerFactory loggerFactory) createInfo)
//        {
//            BusQueueSender busQueueSender = default;
//            try
//            {
//                busQueueSender = new BusQueueSender(createInfo.serviceBusConnectionStringBuilder, createInfo.mode, createInfo.operationTimeout, createInfo.retryPolicy, createInfo.loggerFactory);

//            }
//            catch { }

//            return busQueueSender;
//        }

//        public static IBusQueueSender CreateBusQueueSender(this IBusQueueSender queueSender)
//        {
//            try
//            {
//                queueSender.CreateClient();
//            }
//            catch { }

//            return queueSender;
//        }

//        public static async Task ExcuteSendAsync(this IBusQueueSender queueSender, Action<IBusQueueSender, CancellationToken> action, CancellationToken cancellationToken = default)
//        {
//            var initializeBlock = new TransformBlock<IBusQueueSender, IBusQueueSender>((qs) =>
//            {
//                try
//                {
//                    qs.CreateClient();
//                }
//                catch { }

//                return qs;
//            },
//           new ExecutionDataflowBlockOptions()
//           {
//               CancellationToken = cancellationToken
//           });

//            var sendBlock = new ActionBlock<IBusQueueSender>((sender) =>
//            {
//                action(sender, cancellationToken);
//            },
//            new ExecutionDataflowBlockOptions()
//            {
//                CancellationToken = cancellationToken
//            });

//            initializeBlock.LinkTo(sendBlock, (sender) =>
//            {
//                return sender != null && sender.QueueClient != null;
//            });

//            await initializeBlock.SendAsync(queueSender)
//                       .ConfigureAwait(continueOnCapturedContext: false);
//        }
//    }
//}
