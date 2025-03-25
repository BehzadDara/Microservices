using Consul;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using RabbitMQ.Client;
using ServiceB;
using ServiceB.Consumers;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ServiceBDBContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

builder.Services.AddHostedService<ModelA1Consumer>();

var redisConnection = builder.Configuration["Redis:ConnectionString"]!;
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(cfg =>
{
    cfg.Address = new Uri(builder.Configuration["Consul:Url"]!);
}));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ServiceBDBContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying migrations of ServiceB database.");
    }
}

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

var consulClient = app.Services.GetRequiredService<IConsulClient>();

var registration = new AgentServiceRegistration
{
    ID = "ServiceB",
    Name = "ServiceB",
    Address = builder.Configuration["HealthCheck:Address"],
    Port = int.Parse(builder.Configuration["HealthCheck:Port"]!),
    Check = new AgentServiceCheck
    {
        HTTP = $"https://{builder.Configuration["HealthCheck:Address"]}:{int.Parse(builder.Configuration["HealthCheck:Port"]!)}/healthz",
        Interval = TimeSpan.FromSeconds(10)
    }
};

await consulClient.Agent.ServiceRegister(registration);

app.UseHttpMetrics();
app.MapMetrics();

app.Run();
