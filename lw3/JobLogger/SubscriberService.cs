using System;
using System.Text;
using System.Linq;
using NATS.Client;
using NATS.Client.Rx;
using NATS.Client.Rx.Ops;
using StackExchange.Redis;

namespace Subscriber
{
    class SubscriberService
    {
        private readonly IDatabase _database;

        public SubscriberService()
        {
            string redisHostValue = Environment.GetEnvironmentVariable("REDIS_HOST");
            string redisPortValue = Environment.GetEnvironmentVariable("REDIS_PORT");
            string redisHost = redisHostValue == null ? "localhost" : redisHostValue;
            string redisPort = redisPortValue == null ? "6379" : redisPortValue;

            _database = ConnectionMultiplexer.Connect(redisHost + ":" + redisPort).GetDatabase();
        }

        public void Run(IConnection connection)
        {
            string busValue = Environment.GetEnvironmentVariable("NATS_BUS");
            string bus = busValue == null ? "events" : busValue;
            var publishers = connection.Observe(bus)
                    .Where(m => m.Data?.Any() == true)
                    .Select(m => Encoding.Default.GetString(m.Data));

            publishers.Subscribe(id =>
            {
                string description = _database.StringGet(id);
                Console.WriteLine($"id: {id}; description: {description}");
            });
        }
    }
}
