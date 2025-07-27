using System.Reflection;
using Landia.Api.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Landia.Api.Startup;

public static class Extensions
{
    private static Assembly Assembly { get; }

    static Extensions()
    {
        Assembly = Assembly.GetExecutingAssembly();
    }

    public static IServiceCollection AddEndpoints(this IServiceCollection serviceCollection)
    {
        var listEndpointTypes = Assembly.GetTypes().Where(x => typeof(IEndpoint).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract).ToList();

        foreach (var endpointType in listEndpointTypes)
        {
            serviceCollection.AddScoped(endpointType);
        }

        return serviceCollection;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var listEndpointTypes = Assembly.GetTypes().Where(x => typeof(IEndpoint).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract).ToList();
        var endpointScope = app.ServiceProvider.CreateScope();
        var endpointRouteBuilder = app.MapGroup("/v1");

        foreach (var endpointType in listEndpointTypes)
        {
            var requiredService = endpointScope.ServiceProvider.GetRequiredService(endpointType);

            if (requiredService is IEndpoint service)
            {
                service.Map(endpointRouteBuilder);
            }
        }

        return app;
    }

    public static IApplicationBuilder Execute(this WebApplication app, Delegate action)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var parameters = action.Method.GetParameters().Select(p => serviceProvider.GetRequiredService(p.ParameterType)).ToArray();
        var result = action.DynamicInvoke(parameters);

        if (result is Task task)
        {
            task.GetAwaiter().GetResult();
        }

        return app;
    }
}