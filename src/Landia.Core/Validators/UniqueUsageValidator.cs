using Landia.Core.Entities;
using Landia.Core.Interfaces;

namespace Landia.Core.Validators;

public class UniqueUsageValidator : ICouponRuleValidator
{
    public Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(Coupon coupon, string customerId, decimal orderValue, CancellationToken cancellationToken = default)
    {
        if (coupon.IsUniquePerCustomer && coupon.HasBeenUsedByCustomer(customerId))
        {
            return Task.FromResult<(bool, string?)>((false, "Este cupom já foi utilizado por você anteriormente"));
        }

        return Task.FromResult<(bool, string?)>((true, null));
    }
}