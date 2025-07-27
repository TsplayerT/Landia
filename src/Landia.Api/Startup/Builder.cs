using Landia.Api.Middleware;
using Landia.Core.Interfaces;
using Landia.Core.Services;
using Landia.Core.Validators;
using Landia.Infrastructure.Data;
using Landia.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace Landia.Api.Startup;

public static class Builder
{
    public static WebApplicationBuilder Create(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Landia API",
                Version = "v1",
                Description = "API para sistema de cupons de desconto"
            });
        });

        builder.Services.AddDbContext<CouponDbContext>(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("DevelopmentConnection"));
            }
            else
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            }
        });

        // Repository
        builder.Services.AddScoped<ICouponRepository, CouponRepository>();

        // Validators
        builder.Services.AddScoped<ICouponRuleValidator, ActiveCouponValidator>();
        builder.Services.AddScoped<ICouponRuleValidator, MinimumValueValidator>();
        builder.Services.AddScoped<ICouponRuleValidator, ExpirationDateValidator>();
        builder.Services.AddScoped<ICouponRuleValidator, UniqueUsageValidator>();

        // Services
        builder.Services.AddEndpoints();
        builder.Services.AddScoped<ICouponService, CouponService>();

        builder.Services.AddScoped<Seeder>();
        builder.Services.AddSingleton<ExceptionHandlingMiddleware>();

        // Logging
        builder.Services.AddLogging();

        // CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin();
                policy.AllowAnyMethod();
                policy.AllowAnyHeader();
            });
        });

        return builder;
    }
}