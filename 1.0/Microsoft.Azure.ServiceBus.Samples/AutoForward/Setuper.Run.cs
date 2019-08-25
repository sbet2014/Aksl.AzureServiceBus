using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

//https://docs.microsoft.com/zh-cn/azure/service-bus-messaging/service-bus-auto-forwarding

namespace AutoForward
{
    public partial class Setuper
    { 
        public async  Task  RunAsync()
        {
            try
            {
                Console.WriteLine("\nSending messages\n");
                // Create sender and send message M1 into the source topic
                //var topicSender = new MessageSender(AzureServiceBusConnectionString, "AutoForwardSourceTopic");
                var topicSender = new MessageSender(this.ServiceBusConnectionStringBuilder, RetryPolicy.Default);
                await topicSender.SendAsync(CreateMessage("M1"));

                // Create sender and send message M2 directly into the target queue
                var queueSender = new MessageSender(AzureServiceBusConnectionString, "AutoForwardTargetQueue");
                await queueSender.SendAsync(CreateMessage("M2"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Create a new Service Bus message.
        public static Message CreateMessage(string label)
        {
            // Creat1e a Service Bus message.
            var msg = new Message(Encoding.UTF8.GetBytes("This is the body of message \"" + label + "\"."));
            msg.UserProperties.Add("Priority", 1);
            msg.UserProperties.Add("Importance", "High");
            msg.Label = label;
            msg.TimeToLive = TimeSpan.FromSeconds(90);
            return msg;
        }
    }
}
