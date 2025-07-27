namespace Landia.Core.Entities;

public class CouponUsage
{
    public CouponUsage(Guid couponId, string customerId, decimal orderValue, decimal discountApplied)
    {
        Id = Guid.NewGuid();
        CouponId = couponId;
        CustomerId = ValidateCustomerId(customerId);
        OrderValue = ValidateOrderValue(orderValue);
        DiscountApplied = ValidateDiscountApplied(discountApplied);
        UsedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid CouponId { get; private set; }
    public string CustomerId { get; private set; }
    public decimal OrderValue { get; private set; }
    public decimal DiscountApplied { get; private set; }
    public DateTime UsedAt { get; private set; }

    public Coupon Coupon { get; private set; } = null!;

    private static string ValidateCustomerId(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException("ID do cliente não pode ser vazio", nameof(customerId));
        }

        return customerId.Trim();
    }

    private static decimal ValidateOrderValue(decimal orderValue)
    {
        if (orderValue <= 0)
        {
            throw new ArgumentException("Valor do pedido deve ser maior que zero", nameof(orderValue));
        }

        return orderValue;
    }

    private static decimal ValidateDiscountApplied(decimal discountApplied)
    {
        if (discountApplied < 0)
        {
            throw new ArgumentException("Desconto aplicado não pode ser negativo", nameof(discountApplied));
        }

        return discountApplied;
    }
}