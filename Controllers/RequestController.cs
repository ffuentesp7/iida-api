using System.Text;

using Iida.Api.Contexts;
using Iida.Core.CsvHelper;
using Iida.Shared.DataTransferObjects;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using RabbitMQ.Client;

namespace Iida.Api.Controllers;

[AllowAnonymous, ApiController, EnableCors("AllowAll"), Route("api/request")]
public class RequestController : ControllerBase {
	private readonly ILogger _logger;
	private readonly Shared.RabbitMq.Parameters _rabbitMqParameters;
	private readonly AppDbContext _context;
	private readonly ICsvService _csvService;
	public RequestController(ILogger<RequestController> logger, AppDbContext context, Shared.RabbitMq.Parameters rabbitMqParameters, ICsvService csvService) {
		_logger = logger;
		_rabbitMqParameters = rabbitMqParameters;
		_context = context;
		_csvService = csvService;
	}
	[HttpPost("place")]
	public async Task<ActionResult<string>> Place([FromBody] Shared.DataTransferObjects.Request request) {
		var order = new Shared.Models.Order {
			Guid = Guid.NewGuid(),
			Status = "Created",
			Timestamp = DateTimeOffset.UtcNow,
			Start = request.Start,
			End = request.End,
			CloudCover = request.CloudCover,
		};
		var queueRequest = new QueueRequest {
			Guid = order.Guid,
			GeoJson = request.GeoJson,
			Timestamp = order.Timestamp,
			Start = request.Start,
			End = request.End,
			CloudCover = request.CloudCover
		};
		Console.WriteLine($"Guid: {request.Guid}");
		var factory = new ConnectionFactory() { HostName = _rabbitMqParameters.Hostname, UserName = _rabbitMqParameters.Username, Password = _rabbitMqParameters.Password };
		using var connection = factory.CreateConnection();
		using var channel = connection.CreateModel();
		_ = channel.QueueDeclare(queue: _rabbitMqParameters.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
		var serialized = JsonConvert.SerializeObject(queueRequest);
		var body = Encoding.UTF8.GetBytes(serialized);
		channel.BasicPublish(exchange: "", routingKey: _rabbitMqParameters.Queue, body: body);
		_logger.LogInformation("Request sent to queue");
		_ = await _context.AddAsync(order);
		_ = await _context.SaveChangesAsync();
		return Ok($"Order with GUID {order.Guid} placed successfully");
	}
	[HttpGet("status")]
	public async Task<ActionResult<string>> Status(Guid guid) {
		var order = await _context.Orders!.FirstOrDefaultAsync(o => o.Guid == guid);
		return order is null ? NotFound($"Order with GUID {guid} not found") : Ok(order.Status);
	}
	[HttpGet("result")]
	public async Task<ActionResult<Order>> Result(Guid guid) {
		var order = await _context.Orders!.Include(o => o.EvapotranspirationMaps).Include(o => o.MeteorologicalDatas).Include(o => o.SatelliteImages).FirstOrDefaultAsync(o => o.Guid == guid);
		if (order is null) {
			return NotFound($"Order with GUID {guid} not found");
		}
		switch (order.Status) {
			case "Created":
				return Ok("Order in queue");
			case "Processing":
				return Ok("Order processing");
			default:
				var dto = new Order {
					Guid = order.Guid,
					Status = order.Status,
					Timestamp = order.Timestamp,
					Start = order.Start,
					End = order.End,
					CloudCover = order.CloudCover,
					EvapotranspirationMaps = order.EvapotranspirationMaps!.Select(e =>
						new EvapotranspirationMap {
							Guid = e.Guid,
							Timestamp = e.Timestamp,
							Url = e.Url
						}
					).ToList(),
					MeteorologicalDatas = order.MeteorologicalDatas!.Select(m =>
						new MeteorologicalData {
							Guid = m.Guid,
							Timestamp = m.Timestamp,
							Url = m.Url
						}
					).ToList(),
					SatelliteImages = order.SatelliteImages!.Select(s =>
						new SatelliteImage {
							Guid = s.Guid,
							Timestamp = s.Timestamp,
							Url = s.Url
						}
					).ToList()
				};
				return Ok(dto);
		}
	}
	[HttpPost("upload")]
	public async Task<ActionResult<string>> Upload([FromForm] IFormFile file) {

	}
}