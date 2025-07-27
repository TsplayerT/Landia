using Landia.Core.Entities;

namespace Landia.Core.Interfaces;

public interface ICouponRuleValidator
{
    Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(Coupon coupon, string customerId, decimal orderValue, CancellationToken cancellationToken = default);
}