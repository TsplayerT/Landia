using Landia.Core.Entities;
using Landia.Core.Interfaces;

namespace Landia.Core.Validators;

public class ExpirationDateValidator : ICouponRuleValidator
{
    public Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(Coupon coupon, string customerId, decimal orderValue, CancellationToken cancellationToken = default)
    {
        if (coupon.ExpirationDate.HasValue && DateTime.UtcNow > coupon.ExpirationDate.Value)
        {
            return Task.FromResult<(bool, string?)>((false, $"Cupom expirado em {coupon.ExpirationDate.Value:dd/MM/yyyy}"));
        }

        return Task.FromResult<(bool, string?)>((true, null));
    }
}