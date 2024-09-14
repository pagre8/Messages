using API.Data_access;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Debugging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApiDbContext>(options =>
options.UseSqlServer(
    builder.Configuration.GetConnectionString("conn")
));

builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration)=>
loggerConfiguration
.WriteTo.Seq("http://localhost:5341"));

var app = builder.Build();

// Configure the HTTP request pipeline.
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
