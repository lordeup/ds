using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Grpc.Core;
using NATS.Client;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace BackendApi.Services
{
    public class JobService : Job.JobBase
    {
        private readonly static Dictionary<string, string> _jobs = new Dictionary<string, string>();
        private readonly ILogger<JobService> _logger;
        private readonly IConnection _connection;
        private readonly ConnectionMultiplexer _сonnectionMultiplexer;

        public JobService(ILogger<JobService> logger)
        {
            _logger = logger;

            string natsHostValue = Environment.GetEnvironmentVariable("NATS_HOST");
            string natsPortValue = Environment.GetEnvironmentVariable("NATS_PORT");
            string natsHost = natsHostValue == null ? "localhost" : natsHostValue;
            string natsPort = natsPortValue == null ? "4222" : natsPortValue;

            _connection = new ConnectionFactory().CreateConnection("nats://" + natsHost + ":" + natsPort);
            
            string redisHostValue = Environment.GetEnvironmentVariable("REDIS_HOST");
            string redisPortValue = Environment.GetEnvironmentVariable("REDIS_PORT");
            string redisHost = redisHostValue == null ? "localhost" : redisHostValue;
            string redisPort = redisPortValue == null ? "6379" : redisPortValue;

            _сonnectionMultiplexer = ConnectionMultiplexer.Connect(redisHost + ":" + redisPort);
        }

        public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            string id = Guid.NewGuid().ToString();
            var resp = new RegisterResponse { Id = id };
            _jobs[id] = request.Description;

            SaveMessageToDatabase(id, request.Description);
            PublishMessageToNats(id);

            return Task.FromResult(resp);
        }

        private void SaveMessageToDatabase(string id, string description)
        {
          IDatabase database = _сonnectionMultiplexer.GetDatabase();
          database.StringSet(id, description);
        }

        private void PublishMessageToNats(string id)
        {
            string busValue = Environment.GetEnvironmentVariable("NATS_BUS");
            string bus = busValue == null ? "events" : busValue;
            byte[] payload = Encoding.Default.GetBytes(id);
            _connection.Publish(bus, payload);
        }
    }
}