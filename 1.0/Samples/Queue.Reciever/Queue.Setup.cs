using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Aksl.AzureServiceBus;

namespace Queue.Reciever
{
    public partial class QueueReciever
    {
        public ServiceCollection Services { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public  IConfigurationBuilder ConfigurationBuilder { get; private set; }

        public IConfigurationRoot Configuration { get; private set; }

        protected static Dictionary<string, QueueSettings> _queueSettings;

        public ILoggerFactory LoggerFactory { get; protected set; }

        protected ILoggerFactory _loggerFactory;

        protected ILogger _logger;

        protected CancellationTokenSource _cancellationTokenSource;

        private static TimeSpan _totalDuration = TimeSpan.Zero;

        public static QueueReciever Instance = new QueueReciever();

        public QueueReciever()
        {
           
        }

        public void InitializeSetup()
        {
            try
            {
                Services = new ServiceCollection();

                //1.Configuration
                this.ConfigurationBuilder = new ConfigurationBuilder();

                string basePath = Directory.GetCurrentDirectory();

                this.ConfigurationBuilder.SetBasePath(basePath)
                            .AddJsonFile(path: "servicebussettings.json", optional: true, reloadOnChange: false);

                this.Configuration = ConfigurationBuilder.Build();

                var queueConnectionString = this.Configuration["serviceBusSettings:aksl:connectionString"];
                var queueSection = this.Configuration.GetSection("serviceBusSettings");
                this.Services.AddOptions().Configure<Dictionary<string, QueueSettings>>(queueSection);

                //2.Logger
                LoggerFactory = new LoggerFactory()
                                .AddConsole()
                                .AddDebug();

                _logger = LoggerFactory.CreateLogger(nameof(QueueReciever));

                //3.Configure Services
                ////this.Services.AddSingleton<IServiceCollection>(this.Services);
                //this.Services.AddSingleton(this.ConfigurationBuilder);
                //this.Services.AddSingleton(this.LoggerFactory);

                //4.
                this.ServiceProvider = this.Services.BuildServiceProvider();

                //5.
                _queueSettings = ServiceProvider.GetRequiredService<IOptions<Dictionary<string, QueueSettings>>>().Value;

                _cancellationTokenSource = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string AzureServiceBusConnectionString => _queueSettings["aksl"].ConnectionString;

        private string QueueName => _queueSettings["aksl"].QueueName;

        public TimeSpan OperationTimeout => TimeSpan.FromMinutes(_queueSettings["aksl"].OperationTimeout);

        //  private RetryPolicy TokenTimeToLive => double.Parse( ConfigurationManager.AppSettings["TokenTimeToLive"]);
    }
}
