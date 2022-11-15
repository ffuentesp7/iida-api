using System.Text;

using Iida.Api.Contexts;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using RabbitMQ.Client;

namespace Iida.Api.Controllers;

[AllowAnonymous, ApiController, EnableCors("AllowAll"), Route("api/crop-request")]
public class RequestController : ControllerBase {
	private readonly ILogger _logger;
	private readonly Shared.RabbitMq.Parameters _rabbitMqParameters;
	private readonly AppDbContext _context;
	public RequestController(ILogger<RequestController> logger, AppDbContext context, Shared.RabbitMq.Parameters rabbitMqParameters) {
		_logger = logger;
		_rabbitMqParameters = rabbitMqParameters;
		_context = context;
	}
	[HttpPost("place-request")]
	public async Task<ActionResult<string>> PlaceRequest([FromBody] Shared.DataTransferObjects.Request request) {
		var factory = new ConnectionFactory() { HostName = _rabbitMqParameters.Hostname, UserName = _rabbitMqParameters.Username, Password = _rabbitMqParameters.Password };
		using (var connection = factory.CreateConnection())
		using (var channel = connection.CreateModel()) {
			_ = channel.QueueDeclare(queue: _rabbitMqParameters.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
			var serialized = JsonConvert.SerializeObject(request);
			var body = Encoding.UTF8.GetBytes(serialized);
			channel.BasicPublish(exchange: "", routingKey: _rabbitMqParameters.Queue, body: body);
			_logger.LogInformation("Request sent to queue");
		}
		return Ok($"Order placed successfully");
	}
}