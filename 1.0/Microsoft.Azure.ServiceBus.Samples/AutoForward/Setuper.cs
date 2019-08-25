using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Microsoft.Azure.ServiceBus;

namespace AutoForward
{
    public partial class Setuper
    {
        public ServiceCollection Services { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public  IConfigurationBuilder ConfigurationBuilder { get; private set; }

        public IConfigurationRoot Configuration { get; private set; }

        protected static Dictionary<string, ServiceBusSettings> _serviceBusSettings;

        public ILoggerFactory LoggerFactory { get; protected set; }

        protected ILoggerFactory _loggerFactory;

        protected ILogger _logger;

        protected CancellationTokenSource _cancellationTokenSource;

        private static TimeSpan _totalDuration = TimeSpan.Zero;

       public static Setuper Instance = new Setuper();

        public Setuper()
        {
           
        }

        public void InitializeSetup()
        {
            try
            {
                Services = new ServiceCollection();

                //1.Configuration
                this.ConfigurationBuilder = new ConfigurationBuilder();

                string basePath =Environment.CurrentDirectory+ "\\..\\..\\.." ;

                this.ConfigurationBuilder.SetBasePath(basePath)
                            .AddJsonFile(path: "servicebussettings.json", optional: true, reloadOnChange: false);

                this.Configuration = ConfigurationBuilder.Build();

                //var connectionString = this.Configuration["serviceBusSettings:autoForwardTargetQueue:connectionString"];
                //var serviceBusSettingsSection = this.Configuration.GetSection("serviceBusSettings");
                //this.Services.AddOptions().Configure<Dictionary<string, ServiceBusSettings>>(serviceBusSettingsSection);

                this.Services.AddOptions().Configure<Dictionary<string, ServiceBusSettings>>((configureOptions) =>
                {
                    Configuration.Bind("serviceBusSettings", configureOptions);
                });

                //2.Logger
                LoggerFactory = new LoggerFactory()
                                .AddConsole()
                                .AddDebug();

               // _logger = LoggerFactory.CreateLogger(nameof(QueueReciever));

                //3.Configure Services
                ////this.Services.AddSingleton<IServiceCollection>(this.Services);
                //this.Services.AddSingleton(this.ConfigurationBuilder);
                //this.Services.AddSingleton(this.LoggerFactory);

                //4.
                this.ServiceProvider = this.Services.BuildServiceProvider();

                //5.
                _serviceBusSettings = ServiceProvider.GetRequiredService<IOptions<Dictionary<string, ServiceBusSettings>>>().Value;

                _cancellationTokenSource = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string AzureServiceBusConnectionString => _serviceBusSettings["autoForwardSourceTopic"].ConnectionString;

        private ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder =>
                                               new ServiceBusConnectionStringBuilder(AzureServiceBusConnectionString);

        private string ForwardSourceTopicName => _serviceBusSettings["autoForwardSourceTopic"]?.EntityPath;

        private string ForwardTargetQueueName => _serviceBusSettings["autoForwardTargetQueue"]?.EntityPath;

        public TimeSpan OperationTimeout => TimeSpan.FromMinutes(_serviceBusSettings["autoForwardSourceTopic"].OperationTimeout);

        //  private RetryPolicy TokenTimeToLive => double.Parse( ConfigurationManager.AppSettings["TokenTimeToLive"]);
    }
}
