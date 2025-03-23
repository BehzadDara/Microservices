using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using ServiceC;
using ServiceC.Consumers;
using ServiceC.Services;
using ServiceC.Workers;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ServiceCDBContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddSingleton<ShortCodeService>();

var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQ");
builder.Services.AddSingleton<IConnectionFactory>(_ =>
    new ConnectionFactory
    {
        HostName = rabbitMQConfig["HostName"]!,
        UserName = rabbitMQConfig["UserName"]!,
        Password = rabbitMQConfig["Password"]!
    });

builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<IConnectionFactory>();
    return factory.CreateConnectionAsync().Result;
});

builder.Services.AddSingleton(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateChannelAsync().Result;
});

builder.Services.AddHostedService<OutboxWorker>();
builder.Services.AddHostedService<ModelA1Consumer>();

var app = builder.Build();

app.Run();
