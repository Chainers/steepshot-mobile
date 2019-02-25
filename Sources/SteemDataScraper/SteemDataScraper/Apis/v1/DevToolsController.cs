using System.Text;
using Microsoft.AspNetCore.Mvc;
using SteemDataScraper.Services;

namespace SteemDataScraper.Apis.v1
{
    [Route("api/v1/[controller]/[action]")]
    [ApiController]
    public class DevToolsController : ControllerBase
    {
        private readonly ScraperService _scraperService;

        public DevToolsController(ScraperService scraperService)
        {
            _scraperService = scraperService;
        }

        [HttpGet]
        public IActionResult PrintStatus()
        {
            var sb = new StringBuilder();

            sb.AppendLine("[");
            _scraperService.PrintStatus(sb);
            sb.AppendLine("]");

            return Ok(sb.ToString());
        }

        [HttpPost]
        public IActionResult SetMaxWorkerCount([FromBody]byte maxCount)
        {
            _scraperService.ThreadsCount = maxCount;
            return Ok();
        }

        [HttpPost]
        public IActionResult SetBlockRange([FromBody]byte blockRange)
        {
            if (blockRange < 1)
                return BadRequest();

            _scraperService.BlockRange = blockRange;
            return Ok();
        }
    }
}