using Iida.Api;

using Microsoft.OpenApi.Models;

using System.Reflection;

string? mySqlConnectionString;
string? rabbitMqHost;
string? rabbitMqQueue;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment()) {
	mySqlConnectionString = builder.Configuration["MYSQL_CONNECTIONSTRING"];
	rabbitMqHost = builder.Configuration["RABBITMQ_HOST"];
	rabbitMqQueue = builder.Configuration["RABBITMQ_QUEUE"];
} else {
	mySqlConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTIONSTRING");
	rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
	rabbitMqQueue = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE");
}
var parameters = new Parameters {
	MySqlConnectionString = mySqlConnectionString,
	RabbitMqHost = rabbitMqHost,
	RabbitMqQueue = rabbitMqQueue
};
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
	options.SwaggerDoc("v1", new OpenApiInfo {
		Version = "v1",
		Title = "API - International Innovation for Digital Agriculture",
		Description = "Endpoints for ordering crop evapotranspiration requests.",
		Contact = new OpenApiContact {
			Name = "Vicente \"vichoste \" Calderón",
			Url = new Uri("https://www.vichoste.cl")
		}
	});
	var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddCors(options => options.AddPolicy("AllowAll", builder => _ = builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddSingleton<Parameters>(parameters);

var app = builder.Build();
_ = app.UseCors("AllowAll");
if (app.Environment.IsDevelopment()) {
	_ = app.UseSwagger();
	_ = app.UseSwaggerUI();
}
_ = app.UseHttpsRedirection();
_ = app.UseAuthentication();
_ = app.UseAuthorization();
_ = app.MapControllers();
app.Run();