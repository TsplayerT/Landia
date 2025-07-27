using FluentAssertions;
using Landia.Core.Entities;
using Landia.Core.Validators;
using static Landia.Core.Enums;

namespace Landia.UnitTests.Validators;

public class ExpirationDateValidatorTests
{
    private ExpirationDateValidator Validator { get; }

    public ExpirationDateValidatorTests()
    {
        Validator = new ExpirationDateValidator();
    }

    [Fact]
    public async Task ValidateAsync_CouponNotExpired_ShouldReturnValid()
    {
        // Arrange
        var coupon = new Coupon("TEST", DiscountType.Fixed, 10m, expirationDate: DateTime.UtcNow.AddDays(1));

        // Act
        var (isValid, errorMessage) = await Validator.ValidateAsync(coupon, "customer1", 100m);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_CouponExpired_ShouldReturnInvalid()
    {
        // Arrange
        var expirationDate = DateTime.UtcNow.AddDays(-1);
        var coupon = new Coupon("TEST", DiscountType.Fixed, 10m, expirationDate: expirationDate);

        // Act
        var (isValid, errorMessage) = await Validator.ValidateAsync(coupon, "customer1", 100m);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be($"Cupom expirado em {expirationDate:dd/MM/yyyy}");
    }

    [Fact]
    public async Task ValidateAsync_NoExpirationDate_ShouldReturnValid()
    {
        // Arrange
        var coupon = new Coupon("TEST", DiscountType.Fixed, 10m);

        // Act
        var (isValid, errorMessage) = await Validator.ValidateAsync(coupon, "customer1", 100m);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }
}