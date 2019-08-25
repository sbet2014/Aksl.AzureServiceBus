using System;

namespace Queue.Sender
{
    class Program
    {
        static void Main(string[] args)
        {
            QueueSender.Instance.InitializeSetup();
            QueueSender.Instance.SendBatchMessages();

            Console.ReadLine();
        }
    }
}