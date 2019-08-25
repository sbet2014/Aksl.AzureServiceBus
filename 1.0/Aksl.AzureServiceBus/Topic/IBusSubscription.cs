//using System;
//using System.Threading.Tasks;

//using Microsoft.Azure.ServiceBus;
//using Microsoft.Azure.ServiceBus.Core;

//namespace Aksl.AzureServiceBus
//{
//    #region IBusSubscription
//    public interface IBusSubscription
//    {
//        #region Properties
//        ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder
//        {
//            get;
//        }

//        SubscriptionClient Client
//        {
//            get;
//        }
//        #endregion

//        #region IMessageReceiver Methods
//        IBusSubscriptionReciever CreateMessageReceiver(); 
//        #endregion
//    }
//    #endregion
//}
