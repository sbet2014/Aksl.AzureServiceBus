using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Aksl.AzureServiceBus;
using Aksl.Concurrency;

namespace Queue.Sender
{
    public partial class QueueSender
    {
        #region Members

        protected QueueSettings _queueSettings;
        //  protected static Dictionary<string, QueueSettings> _queueSettings;

        protected ILoggerFactory _loggerFactory;
        protected ILogger _logger;

        protected AsyncLock _mutex;
        protected CancellationTokenSource _cancellationTokenSource;

        protected bool _isInitialize;

        private static TimeSpan _totalDuration = TimeSpan.Zero;

        public static QueueSender Instance = new QueueSender();
        #endregion

        #region Constructors
        public QueueSender()
        {
        }

        public void Initialize()
        {
            try
            {
                _isInitialize = true;

                _mutex = new AsyncLock();

                Services = new ServiceCollection();
                this.Services.AddOptions();

                //1.Configuration
                string basePath = Directory.GetCurrentDirectory() + @"\..\..\..\..";

                this.ConfigurationBuilder = new ConfigurationBuilder()
                                               .SetBasePath(basePath)
                                               .AddJsonFile(path: "servicebussettings.json", optional: true, reloadOnChange: false);

                this.Configuration = ConfigurationBuilder.Build();

                var queueConnectionString = this.Configuration["serviceBusSettings:queues:aksl"];
                //var queueSection = this.Configuration.GetSection("serviceBusSettings");
                //this.Services.Configure<Dictionary<string, QueueSettings>>(queueSection);
                //this.Services.Configure<Dictionary<string, QueueSettings>>(
                //(settings) => 
                //{
                //    this.Configuration.Bind("serviceBusSettings", settings);
                //});

                //2.Logger
                Services.AddLogging(builder =>
                {
                    var loggingSection = this.Configuration.GetSection("Logging");
                    var includeScopes = loggingSection.GetValue<bool>("IncludeScopes");

                    builder.AddConfiguration(loggingSection);

                    //加入一个ConsoleLoggerProvider
                    builder.AddConsole(consoleLoggerOptions =>
                    {
                        consoleLoggerOptions.IncludeScopes = includeScopes;
                    });

                    //加入一个DebugLoggerProvider
                    builder.AddDebug();
                });

                //3.Configure Services
                ////this.Services.AddSingleton<IServiceCollection>(this.Services);
                //this.Services.AddSingleton(this.ConfigurationBuilder);
                //this.Services.AddSingleton(this.LoggerFactory);

                //4.
                this.ServiceProvider = this.Services.BuildServiceProvider();

                _loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
                _logger = _loggerFactory.CreateLogger<QueueSender>();

                //5.
             //   _queueSettings = ServiceProvider.GetRequiredService<IOptions<Dictionary<string, QueueSettings>>>().Value;

                _cancellationTokenSource = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region Properties
        public ServiceCollection Services { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public IConfigurationBuilder ConfigurationBuilder { get; private set; }

        public IConfigurationRoot Configuration { get; private set; }

        public ILoggerFactory LoggerFactory => _loggerFactory;

     //   private string AzureServiceBusConnectionString => _queueSettings["aksl"].ServiceBusConnectionStringBuilder.GetNamespaceConnectionString();

        //  private string QueueName => _queueSettings["aksl"].QueueName;

        //   public TimeSpan OperationTimeout => TimeSpan.FromMinutes(_queueSettings["aksl"].OperationTimeout);

        //  private RetryPolicy TokenTimeToLive => double.Parse( ConfigurationManager.AppSettings["TokenTimeToLive"]);

        #endregion
    }
}
