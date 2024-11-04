using WebSocket_Server.DataAccess;
using Serilog;
using WebSocket_Server.Infrastructure.Messaging;
using WebSocket_Server.Infrastructure.Services;
using WebSocket_Server.Interfaces;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//once per request
builder.Services.AddScoped<ICassandraAccess, CassandraAccess>();

builder.Services.AddScoped<IMessageService, MessageService>();

//once per app lifetime
builder.Services.AddSingleton<IRabbitAccess, RabbitAccess>();


builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
loggerConfiguration
.WriteTo.Seq("http://localhost:5341")
.Enrich.FromLogContext()
.Enrich.WithProperty("Server","WebSocketServer")
.MinimumLevel.Debug()
.WriteTo.Console());


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.Urls.Add("https://localhost:5001");

var webSocketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
};

app.UseWebSockets(webSocketOptions);

app.UseRouting();

app.MapControllers();

app.Run();
