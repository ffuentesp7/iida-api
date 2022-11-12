namespace Iida.Api;

/// <summary>
/// Program parameters.
/// </summary>
public class Parameters {
	/// <summary>
	/// Database's connection string.
	/// </summary>
	public string? MySqlConnectionString { get; set; }
	/// <summary>
	/// RabbitMQ's host name.
	/// </summary>
	public string? RabbitMqHost { get; set; }
	/// <summary>
	/// RabbitMQ's queue name.
	/// </summary>
	public string? RabbitMqQueue { get; set; }
}