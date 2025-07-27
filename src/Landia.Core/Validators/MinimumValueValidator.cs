using Landia.Core.Entities;
using Landia.Core.Interfaces;

namespace Landia.Core.Validators;

public class MinimumValueValidator : ICouponRuleValidator
{
    public Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(Coupon coupon, string customerId, decimal orderValue, CancellationToken cancellationToken = default)
    {
        if (coupon.MinimumOrderValue.HasValue && orderValue < coupon.MinimumOrderValue.Value)
        {
            return Task.FromResult<(bool, string?)>((false, $"Valor mínimo do pedido deve ser R$ {coupon.MinimumOrderValue.Value:F2}"));
        }

        return Task.FromResult((true, (string?)null));
    }
}