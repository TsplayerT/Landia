using static Landia.Core.Enums;

namespace Landia.Core.Entities;

public class Coupon
{
    public Guid Id { get; }
    public string Code { get; private set; }
    public DiscountType DiscountType { get; }
    public decimal DiscountValue { get; }
    public decimal? MinimumOrderValue { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsUniquePerCustomer { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public List<CouponUsage> Usages { get; }

    public Coupon(string code, DiscountType discountType, decimal discountValue, decimal? minimumOrderValue = null, DateTime? expirationDate = null, bool isUniquePerCustomer = false)
    {
        Id = Guid.NewGuid();
        Code = ValidateCode(code);
        DiscountType = discountType;
        DiscountValue = ValidateDiscountValue(discountValue, discountType);
        MinimumOrderValue = minimumOrderValue;
        ExpirationDate = expirationDate;
        IsActive = true;
        IsUniquePerCustomer = isUniquePerCustomer;
        CreatedAt = DateTime.UtcNow;
        Usages = new List<CouponUsage>();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddUsage(string customerId, decimal orderValue, decimal discountApplied)
    {
        var usage = new CouponUsage(Id, customerId, orderValue, discountApplied);

        Usages.Add(usage);
    }

    public bool HasBeenUsedByCustomer(string customerId)
    {
        return Usages.Any(u => u.CustomerId == customerId);
    }

    public decimal CalculateDiscount(decimal orderValue)
    {
        return DiscountType switch
        {
            DiscountType.Fixed => Math.Min(DiscountValue, orderValue),
            DiscountType.Percentage => orderValue * (DiscountValue / 100),
            _ => throw new InvalidOperationException($"Tipo de desconto não suportado: {DiscountType}")
        };
    }

    private static string ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Código do cupom não pode ser vazio", nameof(code));
        }

        if (code.Length > 50)
        {
            throw new ArgumentException("Código do cupom deve ter no máximo 50 caracteres", nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }

    private static decimal ValidateDiscountValue(decimal value, DiscountType type)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Valor do desconto deve ser maior que zero", nameof(value));
        }

        if (type == DiscountType.Percentage && value > 100)
        {
            throw new ArgumentException("Desconto percentual não pode ser maior que 100%", nameof(value));
        }

        return value;
    }
}