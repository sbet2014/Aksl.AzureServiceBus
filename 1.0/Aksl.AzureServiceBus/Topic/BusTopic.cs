//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//using Microsoft.Extensions.Logging;
//using Microsoft.Azure.ServiceBus;
//using Microsoft.Azure.ServiceBus.Core;
//using Newtonsoft.Json;

////Install-Package Microsoft.Azure.ServiceBus -Pre 

//namespace Aksl.AzureServiceBus
//{
//    public class BusTopic : IBusTopic
//    {
//        #region Members
//        protected ServiceBusConnectionStringBuilder _serviceBusConnectionStringBuilder ;

//        protected TopicClient _client;

//        protected ReceiveMode _receiveMode;

//        protected RetryPolicy _retryPolicy;

//        protected ILoggerFactory _loggerFactory;

//        protected ILogger _logger;
//        #endregion

//        #region Constructors
//        public BusTopic(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder,  RetryPolicy retryPolicy = null, ILoggerFactory loggerFactory = null) =>
//                           InitializeTopic(serviceBusConnectionStringBuilder,  retryPolicy, loggerFactory);

//        protected void InitializeTopic(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, RetryPolicy retryPolicy, ILoggerFactory loggerFactory)
//        {
//            ServiceBusConnectionStringBuilder = serviceBusConnectionStringBuilder;

//            _retryPolicy = retryPolicy ?? RetryPolicy.Default;
//            CreateTopicClient();

//            _loggerFactory = loggerFactory;
//            _logger = loggerFactory?.CreateLogger(nameof(BusQueue));
//        }
//        #endregion

//        #region IBusTopic Properties
//        public ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder
//        {
//            get => _serviceBusConnectionStringBuilder ??
//                  throw new ArgumentNullException("service bus connectionstring builder");
//            set => _serviceBusConnectionStringBuilder = value ??
//                      throw new ArgumentNullException("service bus connectionstring builder");
//        }

//       private string  ConnectionString => ServiceBusConnectionStringBuilder.ToString();

//        private string TopicName => ServiceBusConnectionStringBuilder.EntityPath;

//        public TopicClient Client
//        {
//            get => _client ??
//                  throw new ArgumentNullException("client");
//            set => _client = value ??
//                  throw new ArgumentNullException("client");
//        }
//        #endregion

//        #region IMessageSender Methods
//        public IBusMessageSender CreateMessageSender()
//        {
//            _logger.LogInformation($"creating a new topic sender");

//            var client = CreateTopicClient();
//            var sender = new BusMessageSender(client,_loggerFactory);
//            return sender;
//        }
//        #endregion

//        #region Helper Methods
//        public TopicClient CreateTopicClient()
//        {
//            var topicClient = new TopicClient(ConnectionString, TopicName, _retryPolicy);
//            return topicClient;
//        }
//        #endregion
//    }
//}
