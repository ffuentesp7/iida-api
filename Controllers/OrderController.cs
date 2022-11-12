using System.Text;

using Iida.Shared.Requests;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using RabbitMQ.Client;

namespace Iida.Api.Controllers;

[AllowAnonymous, ApiController, EnableCors("AllowAll"), Route("api/crop-request")]
public class OrderController : ControllerBase {
	private readonly ILogger _logger;
	public OrderController(ILogger<OrderController> logger) {
		_logger = logger;
	}
	[HttpPost("place-order")]
	public async Task<ActionResult<Order>> PlaceRequest([FromBody] Order request) {
		var factory = new ConnectionFactory() { HostName = "localhost" };
		using (var connection = factory.CreateConnection())
		using (var channel = connection.CreateModel()) {
			var order = JsonConvert.SerializeObject(request);
			var body = Encoding.UTF8.GetBytes(order);
			channel.BasicPublish(exchange: "", routingKey: "iida-queue", body: body);
			_logger.LogInformation("Envia3");
        }
		return this.Ok();
	}
}