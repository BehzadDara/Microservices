using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Configuration.AddJsonFile("ocelot.json");

builder.Services
    .AddOcelot()
    .AddConsul();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/ServiceA/swagger/v1/swagger.json", "ServiceA");
        c.SwaggerEndpoint("/ServiceB/swagger/v1/swagger.json", "ServiceB");

        c.RoutePrefix = string.Empty;
    });
}

await app.UseOcelot();

app.Run();
