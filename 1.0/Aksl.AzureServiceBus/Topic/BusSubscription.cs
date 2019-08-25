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
//    public class BusSubscription : IBusSubscription
//    {
//        #region Members
//        protected string _subscriptionName;

//        protected ServiceBusConnectionStringBuilder _serviceBusConnectionStringBuilder ;

//        protected SubscriptionClient _client;

//        protected ReceiveMode _receiveMode;

//        protected RetryPolicy _retryPolicy;

//        protected ILoggerFactory _loggerFactory;

//        protected ILogger _logger;
//        #endregion

//        #region Constructors
//        public BusSubscription(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, string subscriptionName, ReceiveMode mode = ReceiveMode.PeekLock, RetryPolicy retryPolicy = null, ILoggerFactory loggerFactory = null) =>
//                          InitializeSubscription(serviceBusConnectionStringBuilder, subscriptionName, mode, retryPolicy, loggerFactory);

//        protected void InitializeSubscription(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, string subscriptionName, ReceiveMode mode, RetryPolicy retryPolicy, ILoggerFactory loggerFactory)
//        {
//            ServiceBusConnectionStringBuilder = serviceBusConnectionStringBuilder;

//            SubscriptionName = subscriptionName;
//            _receiveMode = mode;
//            _retryPolicy = retryPolicy ?? RetryPolicy.Default;
//            CreateSubscriptionClient();

//            _loggerFactory = loggerFactory;
//            _logger = loggerFactory?.CreateLogger(nameof(BusSubscription));
//        }
//        #endregion

//        #region IBusQueue Properties
//        public ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder
//        {
//            get => _serviceBusConnectionStringBuilder ??
//                  throw new ArgumentNullException("service bus connectionstring builder");
//            set => _serviceBusConnectionStringBuilder = value ??
//                      throw new ArgumentNullException("service bus connectionstring builder");
//        }

//       private string  ConnectionString => ServiceBusConnectionStringBuilder.ToString();

//        private string TopicPath => ServiceBusConnectionStringBuilder.EntityPath;

//        public string SubscriptionName
//        {
//            get => _subscriptionName ??
//                    throw new ArgumentException("subscription name");
//            set => _subscriptionName = value ??
//                    throw new ArgumentException("subscription name");
//        }

//        public SubscriptionClient Client
//        {
//            get => _client ??
//                  throw new ArgumentNullException("client");
//            set => _client = value ??
//                  throw new ArgumentNullException("client");
//        }
//        #endregion

//        #region IMessageReceiver Methods
//        public IBusSubscriptionReciever CreateMessageReceiver()
//        {
//            _logger.LogInformation($"creating a new subscription receiver");

//            var client = CreateSubscriptionClient();
//            var reciever = new BusSubscriptionReciever(client, _loggerFactory);
//            return reciever;
//        }
      
//        #endregion 

//        #region Helper Methods
//        public SubscriptionClient CreateSubscriptionClient()
//        {
//            var topicClient = new SubscriptionClient(ConnectionString, TopicPath , SubscriptionName, _receiveMode,  _retryPolicy);
//            return topicClient;
//        }
//        #endregion
//    }
//}
