using System;
using Subscriber;
using NATS.Client;

namespace JobLogger
{
    class Program
    {
        public static void Main(string[] args)
        {
            var subscriberService = new SubscriberService();
            bool isCancel = false;

            string natsHostValue = Environment.GetEnvironmentVariable("NATS_HOST");
            string natsPortValue = Environment.GetEnvironmentVariable("NATS_PORT");
            string natsHost = natsHostValue == null ? "localhost" : natsHostValue;
            string natsPort = natsPortValue == null ? "4222" : natsPortValue;

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
