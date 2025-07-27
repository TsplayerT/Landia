using Landia.Core.Entities;
using Landia.Core.Interfaces;

namespace Landia.Core.Validators;

public class ActiveCouponValidator : ICouponRuleValidator
{
    public Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(Coupon coupon, string customerId, decimal orderValue, CancellationToken cancellationToken = default)
    {
        if (!coupon.IsActive)
        {
            return Task.FromResult<(bool, string?)>((false, "Cupom não está ativo"));
        }

        return Task.FromResult<(bool, string?)>((true, null));
    }
}