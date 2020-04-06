using System;
using System.Text;
using System.Linq;
using System.Collections.Generic; 
using NATS.Client;
using NATS.Client.Rx;
using NATS.Client.Rx.Ops;
using StackExchange.Redis;

namespace TextRankCalc
{
    class SubscriberService
    {
        private readonly IDatabase _database;

        private readonly ISet<char> _vowels;

        private readonly ISet<char> _consonants;

        public SubscriberService()
        {
            string redisHost = Environment.GetEnvironmentVariable("REDIS_HOST");
            string redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");

            _database = ConnectionMultiplexer.Connect(redisHost + ":" + redisPort).GetDatabase();
            _vowels = new HashSet<char> { 'a', 'e', 'i', 'o', 'u' };
            _consonants = new HashSet<char> { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
        }

        public int GetCountCharactersCollection(string str, ISet<char> chars)
        {
            return str.ToLower().Count(ch => chars.Contains(ch));
        }

        public void Run(IConnection connection)
        {
            var publishers = connection.Observe(Environment.GetEnvironmentVariable("NATS_BUS"))
                    .Where(m => m.Data?.Any() == true)
                    .Select(m => Encoding.Default.GetString(m.Data));

            publishers.Subscribe(id =>
            {
                string data = _database.StringGet("data_" + id);

                int vowelsCount = GetCountCharactersCollection(data, _vowels);
                int consonantsCount = GetCountCharactersCollection(data, _consonants);
                float rank = consonantsCount != 0 ? ((float)vowelsCount / consonantsCount) : 0;

                _database.StringSet("rank_" + id, rank.ToString());
            });
        }
    }
}
