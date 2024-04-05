using Microsoft.AspNetCore.Mvc;

namespace SocketLabs.NLog.Targets.OpenSearch.AspNetCoreExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet, Route("exception")] 
        public ActionResult LogException()
        {
            try
            {
                var inner = new Exception("Something, something, something, complete");
                throw new Exception("Something, something, something darkside.", inner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dang.");
            }

            return BadRequest();
        }
    }
}
