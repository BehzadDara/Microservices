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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/ServiceA/swagger/v1/swagger.json", "ServiceA");
    c.SwaggerEndpoint("/ServiceB/swagger/v1/swagger.json", "ServiceB");

    c.RoutePrefix = string.Empty;
});

await app.UseOcelot();

app.Run();
