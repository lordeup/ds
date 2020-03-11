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

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FormSubmit(String description)
        {
            if (description == null)
            {
                return View("Error", new ErrorViewModel {RequestId = "Description must be filled"});
            }

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            
            string value = Environment.GetEnvironmentVariable("BACKEND_HOST");
            string host = value == null ? "localhost" : value;
            using var channel = GrpcChannel.ForAddress("http://" + host + ":5000");

            var client = new Job.JobClient(channel);
            var response = await client.RegisterAsync(new RegisterRequest { Description = description });
            
            return View("Task", new TaskViewModel { Id = response.Id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
