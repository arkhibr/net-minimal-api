using System.Reflection;

namespace ProdutosAPI.Features.Common;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpointsFromAssembly(
        this IServiceCollection services, Assembly assembly)
    {
        var endpointTypes = assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && t.IsAssignableTo(typeof(IEndpoint)));

        foreach (var type in endpointTypes)
            services.AddTransient(typeof(IEndpoint), type);

        return services;
    }

    public static WebApplication MapRegisteredEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetServices<IEndpoint>();
        foreach (var endpoint in endpoints)
            endpoint.MapEndpoints(app);

        return app;
    }
}
