using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using System.Net.Http;

namespace AuthenticationBassic.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _client;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Secret()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer{token}");

            var serverResponse = await _client.GetAsync("https://localhost:5001/secret/index");

            var apiResponse = await _client.GetAsync("https://localhost:5001/secret/index");

            return View();
        }
    }
}
