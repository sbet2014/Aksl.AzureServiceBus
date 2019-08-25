
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Aksl.AzureServiceBus.Queue.Configuration
{
    public class QueueSettingsConfigureOptions : IConfigureOptions<QueueSettings>
    {
        private readonly IConfiguration _configuration;

        public QueueSettingsConfigureOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(QueueSettings options)
        {
            LoadDefaultConfigValues(options);
        }

        private void LoadDefaultConfigValues(QueueSettings options)
        {
            if (_configuration == null)
            {
                return;
            }

            var queuesSection = _configuration.GetSection("queues");
            queuesSection.Bind(queuesSection.Key, options);
        }
    }
}