using FluentAssertions;
using Landia.Core.DTOs;
using Landia.Core.Entities;
using Landia.Core.Interfaces;
using Landia.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using static Landia.Core.Enums;

namespace Landia.UnitTests.Services;

public class CouponServiceTests
{
    private Mock<ILogger<CouponService>> MockLogger { get;}
    private Mock<ICouponRepository> MockRepository { get;}
    private CouponService Service { get;}
    private List<ICouponRuleValidator> Validators { get; }

    public CouponServiceTests()
    {
        MockRepository = new Mock<ICouponRepository>();
        MockLogger = new Mock<ILogger<CouponService>>();
        Validators = new List<ICouponRuleValidator>();
        Service = new CouponService(MockRepository.Object, Validators, MockLogger.Object);
    }

    [Fact]
    public async Task CreateCouponAsync_ValidRequest_ShouldReturnCouponResponse()
    {
        // Arrange
        var request = new CreateCouponRequest("TEST10", DiscountType.Percentage, 10m, 50m);

        MockRepository.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        MockRepository.Setup(r => r.CreateAsync(It.IsAny<Coupon>(), It.IsAny<CancellationToken>())).ReturnsAsync((Coupon c, CancellationToken _) => c);

        // Act
        var result = await Service.CreateCouponAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("TEST10");
        result.DiscountType.Should().Be(DiscountType.Percentage);
        result.DiscountValue.Should().Be(10m);
        result.MinimumOrderValue.Should().Be(50m);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCouponAsync_DuplicateCode_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new CreateCouponRequest("DUPLICATE", DiscountType.Fixed, 20m);

        MockRepository.Setup(r => r.ExistsAsync("DUPLICATE", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        await Service.Invoking(s => s.CreateCouponAsync(request)).Should().ThrowAsync<InvalidOperationException>().WithMessage("Já existe um cupom com o código 'DUPLICATE'");
    }

    [Fact]
    public async Task ApplyCouponAsync_ValidCoupon_ShouldReturnSuccess()
    {
        // Arrange
        var coupon = new Coupon("VALID10", DiscountType.Percentage, 10m, 50m);
        var request = new ApplyCouponRequest("VALID10", "customer1", 100m);

        MockRepository.Setup(r => r.GetByCodeAsync("VALID10", It.IsAny<CancellationToken>())).ReturnsAsync(coupon);

        MockRepository.Setup(r => r.UpdateAsync(It.IsAny<Coupon>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Simular validadores que passam
        var mockValidator = new Mock<ICouponRuleValidator>();

        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<Coupon>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>())).ReturnsAsync((true, (string?)null));

        Validators.Add(mockValidator.Object);

        // Act
        var result = await Service.ApplyCouponAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(10m); // 10% de 100 = 10
        result.FinalAmount.Should().Be(90m);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ApplyCouponAsync_InvalidCoupon_ShouldReturnFailure()
    {
        // Arrange
        var request = new ApplyCouponRequest("INVALID", "customer1", 100m);

        MockRepository.Setup(r => r.GetByCodeAsync("INVALID", It.IsAny<CancellationToken>())).ReturnsAsync((Coupon?)null);

        // Act
        var result = await Service.ApplyCouponAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Cupom não encontrado");
        result.DiscountAmount.Should().Be(0);
        result.FinalAmount.Should().Be(0);
    }

    [Fact]
    public async Task ApplyCouponAsync_ValidationFails_ShouldReturnFailureWithErrors()
    {
        // Arrange
        var coupon = new Coupon("TEST10", DiscountType.Percentage, 10m, 100m);
        var request = new ApplyCouponRequest("TEST10", "customer1", 50m);

        MockRepository.Setup(r => r.GetByCodeAsync("TEST10", It.IsAny<CancellationToken>())).ReturnsAsync(coupon);

        // Simular validador que falha
        var mockValidator = new Mock<ICouponRuleValidator>();

        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<Coupon>(), It.IsAny<string>(), It.IsAny<decimal>(),It.IsAny<CancellationToken>())).ReturnsAsync((false, "Valor mínimo não atingido"));

        Validators.Add(mockValidator.Object);

        // Act
        var result = await Service.ApplyCouponAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Cupom inválido");
        result.ValidationErrors.Should().Contain("Valor mínimo não atingido");
    }

    [Fact]
    public async Task GetCouponAsync_ExistingCoupon_ShouldReturnCouponResponse()
    {
        // Arrange
        var coupon = new Coupon("EXISTING", DiscountType.Fixed, 25m);

        MockRepository.Setup(r => r.GetByCodeAsync("EXISTING", It.IsAny<CancellationToken>())).ReturnsAsync(coupon);

        // Act
        var result = await Service.GetCouponAsync("EXISTING");

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("EXISTING");
        result.DiscountValue.Should().Be(25m);
    }

    [Fact]
    public async Task GetCouponAsync_NonExistingCoupon_ShouldReturnNull()
    {
        // Arrange
        MockRepository.Setup(r => r.GetByCodeAsync("NONEXISTENT", It.IsAny<CancellationToken>())).ReturnsAsync((Coupon?)null);

        // Act
        var result = await Service.GetCouponAsync("NONEXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeactivateCouponAsync_ExistingCoupon_ShouldDeactivate()
    {
        // Arrange
        var coupon = new Coupon("DEACTIVATE", DiscountType.Fixed, 10m);

        MockRepository.Setup(r => r.GetByCodeAsync("DEACTIVATE", It.IsAny<CancellationToken>())).ReturnsAsync(coupon);
        MockRepository.Setup(r => r.UpdateAsync(It.IsAny<Coupon>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await Service.DeactivateCouponAsync("DEACTIVATE");

        // Assert
        coupon.IsActive.Should().BeFalse();
        MockRepository.Verify(r => r.UpdateAsync(coupon, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateCouponAsync_NonExistingCoupon_ShouldThrowException()
    {
        // Arrange
        MockRepository.Setup(r => r.GetByCodeAsync("NONEXISTENT", It.IsAny<CancellationToken>())).ReturnsAsync((Coupon?)null);

        // Act & Assert
        await Service.Invoking(s => s.DeactivateCouponAsync("NONEXISTENT")).Should().ThrowAsync<InvalidOperationException>().WithMessage("Cupom com código 'NONEXISTENT' não encontrado");
    }
}