using API_Server.DataAccess;
using API_Server.Interfaces;
using API_Server.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Debugging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICassandraAccess, CassandraAccess>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IMessageService, MessageService>();

builder.Services.AddDbContext<ApiDbContext>(options =>
options.UseSqlServer(
    builder.Configuration.GetConnectionString("conn")
));

builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
loggerConfiguration
.WriteTo.Seq("http://localhost:5341")
.Enrich.FromLogContext()
.Enrich.WithProperty("Server", "APIServer")
.MinimumLevel.Debug()
.WriteTo.Console());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
SelfLog.Enable(Console.Error);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
