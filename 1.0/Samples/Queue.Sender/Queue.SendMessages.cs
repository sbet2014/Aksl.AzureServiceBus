using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

using Aksl.AzureServiceBus;

namespace Queue.Sender
{
    public partial class QueueSender
    {
        public void SendBatchMessages()
        {
            SendBatchMessageAsync().ConfigureAwait(false);
        }

        private async Task SendBatchMessageAsync()
        {
            try
            {
                var messages = Datas.CreateMessages();

                var logger = this.LoggerFactory.CreateLogger($"{ messages.Count()}_Messages_Sender");

                //var connectionStringBuilder = new ServiceBusConnectionStringBuilder(AzureServiceBusConnectionString)
                //{
                //    EntityPath = QueueName
                //};

                //var namespaceConnectionString = connectionStringBuilder.GetNamespaceConnectionString();
                //var entityConnectionString = connectionStringBuilder.GetEntityConnectionString();
             //   var retryPolicy = RetryPolicy.Default;
             //    IBusQueue busQueue =new BusQueue(connectionStringBuilder,ReceiveMode.PeekLock,0.1,  RetryPolicy.Default, this.LoggerFactory);

                //await busQueue
                //  .ExcuteSendAsync(action: async (sender, token) =>
                //  {
                //      try
                //      {
                //          TimeSpan totalDuration = TimeSpan.Zero;

                //          sender.OnSendCallBack = (context) =>
                //          {
                //              if (context.Exception != null)
                //              {
                //                  _logger.LogError($"exception: {context.Exception} when queue send { messages.Count()} messages");
                //              }

                //              if (context.Exception == null && context.ExecutionTime != null)
                //              {
                //                  _logger.LogInformation(
                //                     $"ThreadId:\"{Environment.CurrentManagedThreadId}\" send {messages.Count()} messages to queue:\"{busQueue.Client.Path}\" duration \"{context.ExecutionTime}\"");
                //              }
                //          };

                //          await sender.SendBatchAsync(messages, cancellationToken: _cancellationTokenSource.Token);

                //          logger.LogInformation($"send {messages.Count()} to queue:\"{busQueue.Client.Path}\" duration :\"{ totalDuration}\"");
                //      }
                //      catch (InvalidOperationException ioex)
                //      {
                //          _logger.LogError(0, ioex, "Error while send a message: {0}", ioex.Message);
                //      }
                //      catch (OperationCanceledException opex)
                //      {
                //          _logger.LogError(0, opex, "Error while cancele send a message: {0}", opex.Message);
                //      }
                //      //catch (Exception ex)
                //      //{
                //      //    _logger.LogError(0, ex, "Error while send a message: {0}", ex.Message);
                //      //}
                //  });
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Error while send a message: {0}", ex.Message);
            }
        }
    }
}
