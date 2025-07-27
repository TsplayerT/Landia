using System.ComponentModel.DataAnnotations;
using Landia.Core.Entities;
using static Landia.Core.Enums;

namespace Landia.Core.DTOs;

public record CreateCouponRequest(
    [Required] [StringLength(50)] string Code,
    [Required] DiscountType DiscountType,
    [Required]
    [Range(0.01, double.MaxValue)]
    decimal DiscountValue,
    [Range(0.01, double.MaxValue)] decimal? MinimumOrderValue = null,
    DateTime? ExpirationDate = null,
    bool IsUniquePerCustomer = false
);

public static class CreateCouponRequestExtensions
{
    public static Coupon ToCoupon(this CreateCouponRequest request)
    {
        return new Coupon(request.Code, request.DiscountType, request.DiscountValue, request.MinimumOrderValue, request.ExpirationDate, request.IsUniquePerCustomer);
    }
}