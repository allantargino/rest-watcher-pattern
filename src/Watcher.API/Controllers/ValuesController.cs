using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Watcher.API.Interfaces;

namespace Watcher.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IProvider<string, string> provider;

        public ValuesController(IProvider<string, string> provider)
        {
            this.provider = provider;
        }

        // GET api/values?key=abc
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] string key, [FromHeader] int timeoutMilliseconds = 150000)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));

            try
            {
                string initialStatus = "Waiting";
                await provider.SetAsync(key, initialStatus);

                var cancellationSource = new CancellationTokenSource(timeoutMilliseconds);

                string finalStatus = await provider.WatchAsync(key, cancellationSource.Token);

                return Ok(finalStatus);
            }
            catch (TaskCanceledException ex)
            {
                return StatusCode(504, ex); // 504 Gateway Timeout
            }
        }

        // GET api/values/set?key=abc&value=Finished
        [HttpGet("set")]
        public async Task<ActionResult> Set([FromQuery] string key, [FromQuery] string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(value));

            await provider.SetAndNotifyAsync(key, value);

            return Ok();
        }
    }
}
