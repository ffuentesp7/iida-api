using System.Text;

using Iida.Api.Contexts;
using Iida.Shared.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using RabbitMQ.Client;

namespace Iida.Api.Controllers;

[AllowAnonymous, ApiController, EnableCors("AllowAll"), Route("api/request")]
public class RequestController : ControllerBase {
	private readonly ILogger _logger;
	private readonly Shared.RabbitMq.Parameters _rabbitMqParameters;
	private readonly AppDbContext _context;
	public RequestController(ILogger<RequestController> logger, AppDbContext context, Shared.RabbitMq.Parameters rabbitMqParameters) {
		_logger = logger;
		_rabbitMqParameters = rabbitMqParameters;
		_context = context;
	}
	[HttpPost("place")]
	public async Task<ActionResult<string>> PlaceRequest([FromBody] Shared.DataTransferObjects.Request request) {
		var order = new Order {
			Guid = Guid.NewGuid(),
			Status = "Created",
			Timestamp = DateTimeOffset.UtcNow,
			Start = request.Start,
			End = request.End,
			CloudCover = request.CloudCover,
		};
		request.Guid = order.Guid;
		var factory = new ConnectionFactory() { HostName = _rabbitMqParameters.Hostname, UserName = _rabbitMqParameters.Username, Password = _rabbitMqParameters.Password };
		using var connection = factory.CreateConnection();
		using var channel = connection.CreateModel();
		_ = channel.QueueDeclare(queue: _rabbitMqParameters.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
		var serialized = JsonConvert.SerializeObject(request);
		var body = Encoding.UTF8.GetBytes(serialized);
		channel.BasicPublish(exchange: "", routingKey: _rabbitMqParameters.Queue, body: body);
		_logger.LogInformation("Request sent to queue");
		_ = await _context.AddAsync(order);
		_ = await _context.SaveChangesAsync();
		return Ok($"Order with GUID {order.Guid} placed successfully");
	}
}