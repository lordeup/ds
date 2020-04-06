using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Frontend.Models;
using Grpc.Net.Client;
using BackendApi;
using Grpc.Core;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GrpcChannel _channel;

        public HomeController(ILogger<HomeController> logger)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            _logger = logger;
            _channel = GrpcChannel.ForAddress("http://" + Environment.GetEnvironmentVariable("BACKEND_HOST") + ":5000");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FormSubmit(RegisterRequest request)
        {
            var client = new Job.JobClient(_channel);
            var response = await client.RegisterAsync(request);
            
            return RedirectToAction("TextDetails", response);
        }

        public IActionResult TextDetails(RegisterResponse response)
        {
            var client = new Job.JobClient(_channel);
            var processingResult = client.GetProcessingResult(response);

            var textDetails = new TextDetailsViewModel 
            { 
                Status = processingResult.Status, 
                Rank = processingResult.Rank
            };

            return View("TextDetails", textDetails);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
