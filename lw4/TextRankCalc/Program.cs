using System;
using NATS.Client;

namespace TextRankCalc
{
    class Program
    {
        static void Main(string[] args)
        {
            var subscriberService = new SubscriberService();
            bool isCancel = false;

            string natsHost = Environment.GetEnvironmentVariable("NATS_HOST");
            string natsPort = Environment.GetEnvironmentVariable("NATS_PORT");

            using (IConnection connection = new ConnectionFactory().CreateConnection("nats://" + natsHost + ":" + natsPort))
            {
                subscriberService.Run(connection);
                Console.WriteLine("Events listening started");
                Console.CancelKeyPress += (sender, args) => { isCancel = true; };
                while (!isCancel) 
                {
                }
            }
        }
    }
}
