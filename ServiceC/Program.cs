using RabbitMQ.Client;
using ServiceC.Consumers;
using ServiceC.Publishers;
using ServiceC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ShortCodeService>();

var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQ");
builder.Services.AddSingleton<IConnectionFactory>(_ =>
    new ConnectionFactory
    {
        HostName = rabbitMQConfig["HostName"]!,
        UserName = rabbitMQConfig["UserName"]!,
        Password = rabbitMQConfig["Password"]!,
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

builder.Services.AddSingleton<ModelA1ShortCodePublisher>();
builder.Services.AddHostedService<ModelA1Consumer>();

var app = builder.Build();

app.Run();
