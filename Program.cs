using Iida.Api.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

string? mySqlConnectionString;
string? rabbitMqHostname;
string? rabbitMqPassword;
string? rabbitMqQueue;
string? rabbitMqUsername;
var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment()) {
	mySqlConnectionString = builder.Configuration["MYSQL_CONNECTIONSTRING"];
	rabbitMqHostname = builder.Configuration["RABBITMQ_HOST"];
	rabbitMqPassword = builder.Configuration["RABBITMQ_PASSWORD"];
	rabbitMqQueue = builder.Configuration["RABBITMQ_QUEUE"];
	rabbitMqUsername = builder.Configuration["RABBITMQ_USERNAME"];
} else {
	mySqlConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTIONSTRING");
	rabbitMqHostname = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
	rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
	rabbitMqQueue = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE");
	rabbitMqUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
}
var mySqlParameters = new Iida.Shared.MySql.Parameters {
	ConnectionString = mySqlConnectionString
};
var rabbitMqParameters = new Iida.Shared.RabbitMq.Parameters {
	Hostname = rabbitMqHostname,
	Password = rabbitMqPassword,
	Queue = rabbitMqQueue,
	Username = rabbitMqUsername
};
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString), mySqlOptions => _ = mySqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)), contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);
builder.Services.AddControllers().AddNewtonsoftJson();
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
});
builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddCors(options => options.AddPolicy("AllowAll", builder => _ = builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddSingleton(rabbitMqParameters);
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