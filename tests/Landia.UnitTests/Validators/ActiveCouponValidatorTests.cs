using FluentAssertions;
using Landia.Core.Entities;
using Landia.Core.Validators;
using static Landia.Core.Enums;

namespace Landia.UnitTests.Validators;

public class ActiveCouponValidatorTests
{
    private ActiveCouponValidator Validator { get; }

    public ActiveCouponValidatorTests()
    {
        Validator = new ActiveCouponValidator();
    }

    [Fact]
    public async Task ValidateAsync_ActiveCoupon_ShouldReturnValid()
    {
        // Arrange
        var coupon = new Coupon("TEST", DiscountType.Fixed, 10m);

        // Act
        var (isValid, errorMessage) = await Validator.ValidateAsync(coupon, "customer1", 100m);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_InactiveCoupon_ShouldReturnInvalid()
    {
        // Arrange
        var coupon = new Coupon("TEST", DiscountType.Fixed, 10m);

        coupon.Deactivate();

        // Act
        var (isValid, errorMessage) = await Validator.ValidateAsync(coupon, "customer1", 100m);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Cupom não está ativo");
    }
}