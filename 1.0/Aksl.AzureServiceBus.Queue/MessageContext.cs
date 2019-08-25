using System;

namespace Aksl.AzureServiceBus.Queue
{
    /// <summary>
    /// MessageContext
    /// </summary>
    public class MessageContext
    {
        /// <summary>
        /// The exception that occured in Load.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// If true, the exception will not be rethrown.
        /// </summary>
        public bool Ignore { get; set; } = true;

        //Ö´ÐÐÊ±¼ä
        public TimeSpan ExecutionTime { get; set; }

        public int MessageConunt { get; set; }
    }
}