namespace Landia.Core.DTOs;

public record CouponApplicationResult(
    bool IsValid,
    decimal DiscountAmount,
    decimal FinalAmount,
    string? ErrorMessage = null,
    List<string>? ValidationErrors = null
)
{
    public static CouponApplicationResult Success(decimal discountAmount, decimal finalAmount)
    {
        return new CouponApplicationResult(true, discountAmount, finalAmount);
    }

    public static CouponApplicationResult Failure(string errorMessage, List<string>? validationErrors = null)
    {
        return new CouponApplicationResult(false, 0, 0, errorMessage, validationErrors);
    }
}