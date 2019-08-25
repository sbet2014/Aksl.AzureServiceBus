//using System;
//using System.Threading.Tasks;

//using Microsoft.Azure.ServiceBus;
//using Microsoft.Azure.ServiceBus.Core;
//using Microsoft.Extensions.Logging;

//namespace Aksl.AzureServiceBus
//{
//    #region  IBusSubscriptionReciever
//    public class BusSubscriptionReciever : BusMessageReciever, IBusSubscriptionReciever
//    {
//        public BusSubscriptionReciever(IReceiverClient messageReciever, ILoggerFactory loggerFactory = null) : base(messageReciever, loggerFactory)
//        {
//        }

//        #region Rule Methods
//        public async Task AddRuleAsync(string ruleName, Filter filter)
//        {
//            if (_messageReciever is SubscriptionClient sc)
//            {
//                await sc.AddRuleAsync(ruleName, filter);
//            }
//        }

//        public async Task AddRuleAsync(RuleDescription description)
//        {
//            if (_messageReciever is SubscriptionClient sc)
//            {
//                await sc.AddRuleAsync(description);
//            }
//        }

//        public async Task RemoveRuleAsync(string ruleName)
//        {
//            if (_messageReciever is SubscriptionClient sc)
//            {
//                await sc.RemoveRuleAsync(ruleName);
//            }
//        }
//        #endregion
//    }
//    #endregion
//}
