using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace SyncVsAsync.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FaultToleranceController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public FaultToleranceController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        private const string FaultyService = "https://httpstat.us/500"; // simulate failure
        private const string SlowService = "https://httpstat.us/200?sleep=5000"; // simulate delay

        // 1️⃣ Retry pattern
        [HttpGet("retry")]
        public async Task<IActionResult> RetryDemo()
        {
            int attempts = 0;
            int maxRetries = 3;

            while (attempts < maxRetries)
            {
                attempts++;
                try
                {
                    var response = await _httpClient.GetAsync(FaultyService);
                    response.EnsureSuccessStatusCode();
                    return Ok(new { Message = "Success on attempt " + attempts });
                }
                catch
                {
                    if (attempts >= maxRetries)
                        return StatusCode(500, $"Failed after {attempts} attempts");
                }
            }
            return StatusCode(500, "Unexpected");
        }

        // 2️⃣ Timeout pattern
        [HttpGet("timeout")]
        public async Task<IActionResult> TimeoutDemo()
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(2);
            try
            {
                var response = await _httpClient.GetAsync(SlowService);
                return Ok(await response.Content.ReadAsStringAsync());
            }
            catch (TaskCanceledException)
            {
                return StatusCode(408, "Request timed out");
            }
        }

        // 3️⃣ Fallback pattern
        [HttpGet("fallback")]
        public async Task<IActionResult> FallbackDemo()
        {
            try
            {
                var response = await _httpClient.GetAsync(FaultyService);
                response.EnsureSuccessStatusCode();
                return Ok(await response.Content.ReadAsStringAsync());
            }
            catch
            {
                return Ok("Fallback response executed due to failure");
            }
        }

        // 4️⃣ “Fake Circuit Breaker” pattern
        private static bool CircuitOpen = false;
        [HttpGet("circuitbreaker")]
        public async Task<IActionResult> CircuitBreakerDemo()
        {
            if (CircuitOpen)
                return StatusCode(503, "Circuit is open, service temporarily unavailable");

            try
            {
                var response = await _httpClient.GetAsync(FaultyService);
                response.EnsureSuccessStatusCode();
                CircuitOpen = false;
                return Ok("Service responded successfully");
            }
            catch
            {
                CircuitOpen = true;
                return StatusCode(500, "Service failed, circuit opened");
            }
        }
    }
}
