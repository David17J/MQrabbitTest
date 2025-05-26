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

        public RabbitMqController(ILogger<RabbitMqController> logger)
        {
            _logger = logger;
            _rabbitMqService = new RabbitMqService();
        }

        [HttpGet(Name = "sendMessage")]
        public IActionResult SendMessage()
        {
            _rabbitMqService.SendMessage("test", "Hallo Welt");
            return Ok("Nachricht wurde an RabbitMQ gesendet.");
        }
    }
}
