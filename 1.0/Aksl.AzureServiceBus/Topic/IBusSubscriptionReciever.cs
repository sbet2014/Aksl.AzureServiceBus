//using System;
//using System.Threading.Tasks;

//using Microsoft.Azure.ServiceBus;
//using Microsoft.Azure.ServiceBus.Core;

//namespace Aksl.AzureServiceBus
//{
//    #region  IBusSubscriptionReciever
//    public interface IBusSubscriptionReciever : IBusMessageReciever
//    {
//        #region Rule Methods
//        Task AddRuleAsync(string ruleName, Filter filter);

//        Task AddRuleAsync(RuleDescription description);

//        Task RemoveRuleAsync(string ruleName);
//        #endregion
//    }
//    #endregion
//}
