using System.Text;

using Iida.Shared.Requests;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using RabbitMQ.Client;

namespace Iida.Api.Controllers;

[AllowAnonymous, ApiController, EnableCors("AllowAll"), Route("api/crop-order")]
public class OrderController : ControllerBase {
	private readonly ILogger _logger;
	private readonly Shared.MySql.Parameters _mySqlParameters;
	private readonly Shared.RabbitMq.Parameters _rabbitMqParameters;

	public OrderController(ILogger<OrderController> logger, Shared.MySql.Parameters mySqlParameters, Shared.RabbitMq.Parameters rabbitMqParameters) {
		_logger = logger;
		_mySqlParameters = mySqlParameters;
		_rabbitMqParameters = rabbitMqParameters;
	}

	[HttpPost("place-order")]
	public async Task<ActionResult<Order>> PlaceRequest([FromBody] Order request) {
		var factory = new ConnectionFactory() { HostName = _rabbitMqParameters.Hostname };
		using (var connection = factory.CreateConnection())
		using (var channel = connection.CreateModel()) {
			_ = channel.QueueDeclare(queue: _rabbitMqParameters.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
			var order = JsonConvert.SerializeObject(request);
			var body = Encoding.UTF8.GetBytes(order);
			channel.BasicPublish(exchange: "", routingKey: _rabbitMqParameters.Queue, body: body);
			_logger.LogInformation("Envia3");
		}
		return this.Ok();
	}
}