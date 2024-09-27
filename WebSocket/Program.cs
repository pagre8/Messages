using WebSocket_Server.Data_access;
using Serilog;
using WebSocket_Server.Rabbit_Access;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//once per request
builder.Services.AddScoped<CassandraAccess>();
//once per app lifetime
builder.Services.AddSingleton<RabbitAccess>();

builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
loggerConfiguration
.WriteTo.Seq("http://localhost:5341"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

var webSocketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromSeconds(120),
    ReceiveBufferSize = 4 * 1024
};

app.UseWebSockets(webSocketOptions);

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.UseHttpsRedirection();

Console.WriteLine("Working");
app.Run();
