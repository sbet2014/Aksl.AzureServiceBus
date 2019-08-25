using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Primitives;
using Newtonsoft.Json;

//Install-Package Microsoft.Azure.ServiceBus -Pre 

namespace Aksl.AzureServiceBus
{
    public class AzureQueueService : IAzureQueueService
    {
        #region Members
        protected ServiceBusConnectionStringBuilder _serviceBusConnectionStringBuilder;

        protected static QueueClient _client;

        protected ReceiveMode _receiveMode;

        protected TimeSpan _operationTimeout;

        protected RetryPolicy _retryPolicy;

        protected ILoggerFactory _loggerFactory;

        protected ILogger _logger;

        protected readonly TimeSpan DefaultOperationTimeout = TimeSpan.FromMinutes(1);
        #endregion

        #region Constructors
        public AzureQueueService(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, ReceiveMode mode = ReceiveMode.PeekLock, double operationTimeout = 1, RetryPolicy retryPolicy = null, ILoggerFactory loggerFactory = null) =>
                           InitializeServiceBus(serviceBusConnectionStringBuilder, mode, operationTimeout, retryPolicy, loggerFactory);

        protected void InitializeServiceBus(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, ReceiveMode mode, double operationTimeout, RetryPolicy retryPolicy, ILoggerFactory loggerFactory)
        {
            ServiceBusConnectionStringBuilder = serviceBusConnectionStringBuilder;

            _operationTimeout = operationTimeout <= 0 ? DefaultOperationTimeout : TimeSpan.FromMinutes(operationTimeout);
            _receiveMode = mode;
            _retryPolicy = retryPolicy ?? RetryPolicy.Default;
            
            _loggerFactory = loggerFactory;
            _logger = loggerFactory?.CreateLogger(nameof(AzureQueueService));
        }
        #endregion

        #region Properties
        public ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder
        {
            get => _serviceBusConnectionStringBuilder ??
                             throw new ArgumentNullException("connectionstring is null");
            set => _serviceBusConnectionStringBuilder = value ??
                             throw new ArgumentNullException(nameof(value), "connectionstring is not null");
        }

        private string ConnectionString => ServiceBusConnectionStringBuilder.GetNamespaceConnectionString();

        private string QueueName => ServiceBusConnectionStringBuilder.EntityPath ??
                                             throw new ArgumentNullException("queue name is null");

        public QueueClient QueueClient
        {
            get => _client;
            set => _client = value ??
                        throw new ArgumentNullException(nameof(value), "queue clientg is not null");
        }

        #endregion

        #region Methods
        public IAzureQueueSender CreateQueueSender()
        {
            CreateQueueClient();

            var sender = new AzureQueueSender(_client, _loggerFactory);
            return sender;
        }
        #endregion

        #region Help Methods
        public void CreateQueueClient()
        {
            _logger.LogInformation($"creating a new queue:{this.QueueName} sender");

            var namespaceConnectionString = ServiceBusConnectionStringBuilder.GetNamespaceConnectionString();

            //_client = new QueueClient(namespaceConnectionString, QueueName, _receiveMode, _retryPolicy);
            _client = new QueueClient(ServiceBusConnectionStringBuilder, _receiveMode, _retryPolicy);
        }

        public async Task CloseAsync()
        {
            await _client.CloseAsync();
        }
        #endregion
    }
}
