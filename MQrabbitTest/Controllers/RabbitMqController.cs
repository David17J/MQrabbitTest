using Microsoft.AspNetCore.Mvc;
using MQrabbitTest.Services;

namespace MQrabbitTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RabbitMqController : ControllerBase
    {
        private readonly ILogger<RabbitMqController> _logger;
        private readonly RabbitMqService _rabbitMqService;

        public RabbitMqController(ILogger<RabbitMqController> logger, RabbitMqService rabbitMqService)
        {
            _logger = logger;
            _rabbitMqService = rabbitMqService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto message)
        {
            if (string.IsNullOrEmpty(message?.Content))
            {
                return BadRequest("Message content cannot be empty");
            }

            var result = await _rabbitMqService.SendMessageAsync("test", message.Content);
            
            if (result)
            {
                return Ok("Message sent successfully");
            }
            
            return StatusCode(500, "Failed to send message after multiple retries");
        }
    }

    public class MessageDto
    {
        public string? Content { get; set; }
    }
}
