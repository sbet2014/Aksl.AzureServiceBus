using System;

namespace AutoForward
{
    class Program
    {
        static void Main(string[] args)
        {
            Setuper.Instance.InitializeSetup();
            Setuper.Instance.RunAsync().GetAwaiter().GetResult();

            Console.ReadLine();
        }
    }
}
