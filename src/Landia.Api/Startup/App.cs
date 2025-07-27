using Landia.Api.Endpoints;
using Landia.Api.Middleware;
using Landia.Infrastructure.Data;

namespace Landia.Api.Startup;

public static class App
{
    public static WebApplication Create(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Landia API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseHttpsRedirection();
        app.UseCors();

        app.MapEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.Execute(async (Seeder seeder) =>
            {
                await seeder.GenerateCuponsAsync();
            });
        }

        return app;
    }
}