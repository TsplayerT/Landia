using Landia.Api.Interfaces;

namespace Landia.Api.Endpoints;

public class HealthEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("/health")
            .WithTags("Health")
            .WithOpenApi();

        // GET /api/v1/health
        routeGroupBuilder.MapGet("/", HealthCheck)
            .WithName(nameof(HealthCheck))
            .WithSummary("Verificar a saúde da API")
            .Produces<(string status, DateTime timestamp)>();
    }

    private static IResult HealthCheck()
    {
        return Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        });
    }
}