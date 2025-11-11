//Hi this is Eswar
// Hi Eswar on Branch
// changes from subhash
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;

namespace SyncVsAsync.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public DataController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // 1️⃣ Synchronous version - sequential API calls
        [HttpGet("sync")]
        public IActionResult GetDataSync()
        {
            var stopwatch = Stopwatch.StartNew();

            for (int i = 1; i <= 10; i++) // 🔹 Make 10 requests sequentially
            {
                var response = _httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1").Result;
                var data = response.Content.ReadAsStringAsync().Result;
            }

            stopwatch.Stop();

            return Ok(new
            {
                Type = "Synchronous",
                Message = "10 sequential requests completed",
                TimeTakenMs = stopwatch.ElapsedMilliseconds
            });
        }

        // 2️⃣ FakeAsync version - async methods but still sequential/blocking
        [HttpGet("fake-async")]
        public async Task<IActionResult> GetDataFakeAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            for (int i = 1; i <= 10; i++)
            {
                // 🔹 Using async methods with .Result blocks the thread
                var response = _httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1").Result;
                var data = response.Content.ReadAsStringAsync().Result;
            }

            stopwatch.Stop();

            return Ok(new
            {
                Type = "Fake Async",
                Message = "10 sequential requests using async methods (blocking)",
                TimeTakenMs = stopwatch.ElapsedMilliseconds
            });
        }

        // 3️⃣ Asynchronous version - parallel API calls
        [HttpGet("async")]
        public async Task<IActionResult> GetDataAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            var tasks = new List<Task<string>>();

            for (int i = 1; i <= 10; i++) // 🔹 Make 10 requests concurrently
            {
                tasks.Add(_httpClient.GetStringAsync("https://jsonplaceholder.typicode.com/posts/1"));
            }

            await Task.WhenAll(tasks); // Wait for all to complete

            stopwatch.Stop();

            return Ok(new
            {
                Type = "Asynchronous",
                Message = "10 parallel requests completed",
                TimeTakenMs = stopwatch.ElapsedMilliseconds
            });
        }
    }
}
