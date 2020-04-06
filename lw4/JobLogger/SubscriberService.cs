using System;
using System.Text;
using System.Linq;
using NATS.Client;
using NATS.Client.Rx;
using NATS.Client.Rx.Ops;
using StackExchange.Redis;

namespace JobLogger
{
    class SubscriberService
    {
        private readonly IDatabase _database;

        public SubscriberService()
        {
            string redisHost = Environment.GetEnvironmentVariable("REDIS_HOST");
            string redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");

            _database = ConnectionMultiplexer.Connect(redisHost + ":" + redisPort).GetDatabase();
        }

        public void Run(IConnection connection)
        {
            var publishers = connection.Observe(Environment.GetEnvironmentVariable("NATS_BUS"))
                    .Where(m => m.Data?.Any() == true)
                    .Select(m => Encoding.Default.GetString(m.Data));

            publishers.Subscribe(id =>
            {
                string description = _database.StringGet("description_" + id);
                Console.WriteLine($"id: {id}; description: {description}");
            });
        }
    }
}
