using System.Text;

using Iida.Shared.Requests;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using RabbitMQ.Client;

namespace Iida.Api.Controllers;

/// <summary>
/// Controlador para la gestión de órdenes de cálculo de evapotranspiración.
/// </summary>
[AllowAnonymous, ApiController, EnableCors("AllowAll"), Route("api/crop-order")]
public class OrderController : ControllerBase {
	private readonly ILogger _logger;
	private readonly Parameters _parameters;

	/// <summary>
	/// Constructor del controlador.
	/// </summary>
	/// <param name="logger">Parámetro de inyección del logger del programa.</param>
	/// <param name="parameters">Parámetro de inyección de parámetros del programa.</param>
	public OrderController(ILogger<OrderController> logger, Parameters parameters) {
		_logger = logger;
		_parameters = parameters;
	}
	
	/// <summary>
	/// Crea una orden.
	/// </summary>
	/// <param name="request">JSON order request.</param>
	/// <returns>HTTP status code.</returns>
	[HttpPost("place-order")]
	public async Task<ActionResult<Order>> PlaceRequest([FromBody] Order request) {
		var factory = new ConnectionFactory() { HostName = _parameters.RabbitMqHost };
		using (var connection = factory.CreateConnection())
		using (var channel = connection.CreateModel()) {
			_ = channel.QueueDeclare(queue: _parameters.RabbitMqQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
			var order = JsonConvert.SerializeObject(request);
			var body = Encoding.UTF8.GetBytes(order);
			channel.BasicPublish(exchange: "", routingKey: _parameters.RabbitMqQueue, body: body);
			_logger.LogInformation("Envia3");
        }
		return this.Ok();
	}
}