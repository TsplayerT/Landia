using Landia.Api.Interfaces;
using Landia.Core.DTOs;
using Landia.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Landia.Api.Endpoints;

public class CouponEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var routeGroupBuilder = app.MapGroup("/coupons")
            .WithTags("Coupons")
            .WithOpenApi();

        // GET /api/v1/coupons
        routeGroupBuilder.MapGet("/", GetAllCoupons)
            .WithName("GetAllCoupons")
            .WithSummary("Buscar todos os cupons")
            .Produces<IEnumerable<CouponResponse>>();

        // GET /api/v1/coupons/{code}
        routeGroupBuilder.MapGet("/{code}", GetCouponByCode)
            .WithName("GetCouponByCode")
            .WithSummary("Buscar cupom por código")
            .Produces<CouponResponse>()
            .Produces(404);

        // POST /api/v1/coupons
        routeGroupBuilder.MapPost("/", CreateCoupon)
            .WithName("CreateCoupon")
            .WithSummary("Criar novo cupom")
            .Produces<CouponResponse>(201)
            .Produces<ValidationProblemDetails>(400)
            .Produces<ProblemDetails>(409);

        // POST /api/v1/coupons/apply
        routeGroupBuilder.MapPost("/apply", ApplyCoupon)
            .WithName("ApplyCoupon")
            .WithSummary("Aplicar cupom no checkout")
            .Produces<CouponApplicationResult>()
            .Produces<ValidationProblemDetails>(400);

        // PATCH /api/v1/coupons/{code}/deactivate
        routeGroupBuilder.MapPatch("/{code}/deactivate", DeactivateCoupon)
            .WithName("DeactivateCoupon")
            .WithSummary("Desativar cupom")
            .Produces(204)
            .Produces<ProblemDetails>(404);
    }

    private static async Task<IResult> GetAllCoupons(ICouponService couponService, CancellationToken cancellationToken)
    {
        var coupons = await couponService.GetAllCouponsAsync(cancellationToken);

        return Results.Ok(coupons);
    }

    private static async Task<IResult> GetCouponByCode([FromRoute] string code, ICouponService couponService, CancellationToken cancellationToken)
    {
        var coupon = await couponService.GetCouponAsync(code, cancellationToken);

        if (coupon != null)
        {
            return Results.Ok(coupon);
        }

        return Results.NotFound(new
        {
            message = "Cupom não encontrado"
        });
    }

    private static async Task<IResult> CreateCoupon([FromBody] CreateCouponRequest request, ICouponService couponService, CancellationToken cancellationToken)
    {
        try
        {
            var coupon = await couponService.CreateCouponAsync(request, cancellationToken);

            return Results.Created($"/api/v1/coupons/{coupon.Code}", coupon);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new
            {
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    private static async Task<IResult> ApplyCoupon([FromBody] ApplyCouponRequest request, ICouponService couponService, CancellationToken cancellationToken)
    {
        try
        {
            var result = await couponService.ApplyCouponAsync(request, cancellationToken);

            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    private static async Task<IResult> DeactivateCoupon([FromRoute] string code, ICouponService couponService, CancellationToken cancellationToken)
    {
        try
        {
            await couponService.DeactivateCouponAsync(code, cancellationToken);

            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new
            {
                message = ex.Message
            });
        }
    }
}