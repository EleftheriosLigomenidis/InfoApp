using Hangfire;
using IPInfoApp.API.Extentions;
using IPInfoApp.API.Middlewares;
using IPInfoApp.Business.Options;
using IPInfoApp.Business.Services;
using IPInfoApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(connectionString));
// we want a single instance of redis throughout our application as a single source of cached data.
// aslo it  is less resource consuming
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddHangfire(builder.Configuration);

if (string.IsNullOrEmpty(redisConnectionString))
{
    throw new ArgumentNullException(nameof(redisConnectionString), "Redis connection string is not configured.");
}

builder.Services.AddSingleton<IConnectionMultiplexer>(c =>
{
    var options = ConfigurationOptions.Parse(redisConnectionString);
    return ConnectionMultiplexer.Connect(options);
});

var ip2cOptions = new HttpIP2COptions();
builder.Services.Configure<HttpIP2COptions>(builder.Configuration.GetSection(HttpIP2COptions.Key));
builder.Configuration.GetSection(HttpIP2COptions.Key).Bind(ip2cOptions);
builder.Services.AddHttpClient<Ip2cService>(client =>
{
    client.BaseAddress = new Uri(ip2cOptions.Uri);
});

builder.Services.AddServiceCollections();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Services.EnqueueBackgroundJobs(builder.Configuration);
app.UseHttpsRedirection();
app.UseHangfireDashboard();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapControllers();

app.Run();
