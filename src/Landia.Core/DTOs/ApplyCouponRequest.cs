using System.ComponentModel.DataAnnotations;

namespace Landia.Core.DTOs;

public record ApplyCouponRequest(
    [Required] [StringLength(50)] string CouponCode,
    [Required] string CustomerId,
    [Required]
    [Range(0.01, double.MaxValue)]
    decimal OrderValue
);