using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UploadImageGCP.Services;

namespace UploadImageGCP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PubSubController : ControllerBase
    {
        private readonly PubSubPublisherService _publisherService;

        public PubSubController(PubSubPublisherService publisherService)
        {
            _publisherService = publisherService;
        }

        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] string message)
        {
            var messageId = await _publisherService.PublishMessageAsync(message);
            return Ok(new { MessageId = messageId });
        }
    }
}
