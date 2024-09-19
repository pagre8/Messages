using WebSocket_Server.Data_access;
using Serilog;
using Serilog.Debugging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<CassandraAccess>();

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


app.Run();
