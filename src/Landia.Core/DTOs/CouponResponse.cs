using Landia.Core.Entities;
using static Landia.Core.Enums;

namespace Landia.Core.DTOs;

public record CouponResponse(
    Guid Id,
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderValue,
    DateTime? ExpirationDate,
    bool IsActive,
    bool IsUniquePerCustomer,
    DateTime CreatedAt,
    int UsageCount
);

public static class CouponResponseExtensions
{
    public static CouponResponse ToResponse(this Coupon coupon)
    {
        return new CouponResponse(
            coupon.Id,
            coupon.Code,
            coupon.DiscountType,
            coupon.DiscountValue,
            coupon.MinimumOrderValue,
            coupon.ExpirationDate,
            coupon.IsActive,
            coupon.IsUniquePerCustomer,
            coupon.CreatedAt,
            coupon.Usages.Count
        );
    }
}