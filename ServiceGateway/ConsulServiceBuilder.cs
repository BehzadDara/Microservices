using Consul;
using Ocelot.Logging;
using Ocelot.Provider.Consul.Interfaces;
using Ocelot.Provider.Consul;

namespace ServiceGateway;

public class ConsulServiceBuilder(
    IHttpContextAccessor contextAccessor, 
    IConsulClientFactory clientFactory, 
    IOcelotLoggerFactory loggerFactory
    ) : DefaultConsulServiceBuilder(contextAccessor, clientFactory, loggerFactory)
{
    protected override string GetDownstreamHost(ServiceEntry entry, Node node)
        => entry.Service.Address;
}