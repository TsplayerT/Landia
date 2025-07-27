using Landia.Core.DTOs;

namespace Landia.Core.Interfaces;

public interface ICouponService
{
    Task<CouponResponse> CreateCouponAsync(CreateCouponRequest request, CancellationToken cancellationToken = default);

    Task<CouponApplicationResult> ApplyCouponAsync(ApplyCouponRequest request, CancellationToken cancellationToken = default);

    Task<CouponResponse?> GetCouponAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<CouponResponse>> GetAllCouponsAsync(CancellationToken cancellationToken = default);
    Task DeactivateCouponAsync(string code, CancellationToken cancellationToken = default);
}