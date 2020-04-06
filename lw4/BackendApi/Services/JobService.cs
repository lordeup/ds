using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
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
        private readonly IDatabase _database;

        private const int DELAY = 1000;
        private const int COUNT_REQUEST_RETRIES = 5;

        public JobService(ILogger<JobService> logger)
        {
            _logger = logger;

            string natsHost = Environment.GetEnvironmentVariable("NATS_HOST");
            string natsPort = Environment.GetEnvironmentVariable("NATS_PORT");

            _connection = new ConnectionFactory().CreateConnection("nats://" + natsHost + ":" + natsPort);
            
            string redisHost = Environment.GetEnvironmentVariable("REDIS_HOST");
            string redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");

            _сonnectionMultiplexer = ConnectionMultiplexer.Connect(redisHost + ":" + redisPort);
            _database = _сonnectionMultiplexer.GetDatabase();
        }

        public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            string id = Guid.NewGuid().ToString();
            var resp = new RegisterResponse { Id = id };
            _jobs[id] = request.Description;

            SaveMessageToDatabase(id, request);
            PublishMessageToNats(id);

            return Task.FromResult(resp);
        }

        public override Task<ProcessingResult> GetProcessingResult(RegisterResponse response, ServerCallContext context)
        {
            var processingResult = new ProcessingResult 
            {
                Status = ProcessingStatus.InProcess,
                Rank = "",
            };

            for(int i = 0; i < COUNT_REQUEST_RETRIES; ++i)
            {
                string rank = _database.StringGet("rank_" + response.Id);
                if (rank != null)
                {
                    processingResult.Status = ProcessingStatus.Completed;
                    processingResult.Rank = rank;
                    break;
                }
                Thread.Sleep(DELAY);
            }
            
            return Task.FromResult(processingResult);
        }

        private void SaveMessageToDatabase(string id, RegisterRequest request)
        {
          _database.StringSet("description_" + id, request.Description);
          _database.StringSet("data_" + id, request.Data);
        }

        private void PublishMessageToNats(string id)
        {
            byte[] payload = Encoding.Default.GetBytes(id);
            _connection.Publish(Environment.GetEnvironmentVariable("NATS_BUS"), payload);
        }
    }
}