using Consul;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using ServiceGateway;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var environmentName = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"ocelot.{environmentName}.json");

builder.Services
    .AddOcelot()
    .AddConsul<ConsulServiceBuilder>();

builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(cfg =>
{
    cfg.Address = new Uri(builder.Configuration["Consul:Url"]!);
}));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/ServiceA/swagger/v1/swagger.json", "ServiceA");
    c.SwaggerEndpoint("/ServiceB/swagger/v1/swagger.json", "ServiceB");

    c.RoutePrefix = string.Empty;
});

app.UseHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

var consulClient = app.Services.GetRequiredService<IConsulClient>();

var registration = new AgentServiceRegistration
{
    ID = "ServiceGateway",
    Name = "ServiceGateway",
    Address = builder.Configuration["HealthCheck:Address"],
    Port = int.Parse(builder.Configuration["HealthCheck:Port"]!),
    Check = new AgentServiceCheck
    {
        HTTP = $"http://{builder.Configuration["HealthCheck:Address"]}:{int.Parse(builder.Configuration["HealthCheck:Port"]!)}/healthz",
        Interval = TimeSpan.FromSeconds(10)
    }
};

await consulClient.Agent.ServiceRegister(registration);

await app.UseOcelot();

app.Run();
