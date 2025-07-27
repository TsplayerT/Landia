using FluentAssertions;
using Landia.Core.Entities;
using Landia.Core.Validators;
using static Landia.Core.Enums;

namespace Landia.UnitTests.Validators;

public class MinimumValueValidatorTests
{
    private MinimumValueValidator Validator { get; }

    public MinimumValueValidatorTests()
    {
        Validator = new MinimumValueValidator();
    }

    [Fact]
    public async Task ValidateAsync_OrderValueMeetsMinimum_ShouldReturnValid()
    {
        // Arrange
        var coupon = new Coupon("TEST", DiscountType.Fixed, 10m, 50m);
        var orderValue = 100m;

        // Act
        var (isValid, errorMessage) = await Validator.ValidateAsync(coupon, "customer1", orderValue);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_OrderValueBelowMinimum_ShouldReturnInvalid()
    {
        // Arrange
        var coupon = new Coupon("TEST", DiscountType.Fixed, 10m, 50m);
        var orderValue = 30m;

        // Act
        var (isValid, errorMessage) = await Validator.ValidateAsync(coupon, "customer1", orderValue);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Valor mínimo do pedido deve ser R$ 50,00");
    }

    [Fact]
    public async Task ValidateAsync_NoMinimumValueSet_ShouldReturnValid()
    {
        // Arrange
        var coupon = new Coupon("TEST", DiscountType.Fixed, 10m);
        var orderValue = 10m;

        // Act
        var (isValid, errorMessage) = await Validator.ValidateAsync(coupon, "customer1", orderValue);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }
}