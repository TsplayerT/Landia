using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Landia.Api;
using Landia.Core.DTOs;
using Landia.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static Landia.Core.Enums;

namespace Landia.IntegrationTests;

public class CouponEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private HttpClient Client { get; }
    private WebApplicationFactory<Program> Factory { get; }

    public CouponEndpointsTests(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove o DbContext existente
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CouponDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Adicionar InMemory database para testes
                services.AddDbContext<CouponDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });
            });
        });

        Client = Factory.CreateClient();
    }

    #region Create Coupon Tests

    [Fact]
    public async Task CreateCoupon_ValidFixedDiscountRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateCouponRequest("FIXED20", DiscountType.Fixed, 20m, MinimumOrderValue: 100m, ExpirationDate: DateTime.UtcNow.AddDays(30), IsUniquePerCustomer: false);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().EndWith("/api/coupons/FIXED20");

        var coupon = await response.Content.ReadFromJsonAsync<CouponResponse>();

        coupon.Should().NotBeNull();
        coupon.Code.Should().Be("FIXED20");
        coupon.DiscountType.Should().Be(DiscountType.Fixed);
        coupon.DiscountValue.Should().Be(20m);
        coupon.MinimumOrderValue.Should().Be(100m);
        coupon.IsUniquePerCustomer.Should().BeFalse();
        coupon.IsActive.Should().BeTrue();
        coupon.ExpirationDate.Should().NotBeNull();
        coupon.UsageCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateCoupon_ValidPercentageDiscountRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateCouponRequest("PERCENT15", DiscountType.Percentage, 15m, MinimumOrderValue: 50m, IsUniquePerCustomer: true);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var coupon = await response.Content.ReadFromJsonAsync<CouponResponse>();

        coupon.Should().NotBeNull();
        coupon.Code.Should().Be("PERCENT15");
        coupon.DiscountType.Should().Be(DiscountType.Percentage);
        coupon.DiscountValue.Should().Be(15m);
        coupon.MinimumOrderValue.Should().Be(50m);
        coupon.IsUniquePerCustomer.Should().BeTrue();
        coupon.ExpirationDate.Should().BeNull();
    }

    [Fact]
    public async Task CreateCoupon_DuplicateCode_ShouldReturnConflict()
    {
        // Arrange
        var request1 = new CreateCouponRequest("DUPLICATE", DiscountType.Fixed, 20m);
        var request2 = new CreateCouponRequest("DUPLICATE", DiscountType.Percentage, 10m);

        // Act
        var firstResponse = await Client.PostAsJsonAsync("/api/coupons", request1);
        var secondResponse = await Client.PostAsJsonAsync("/api/coupons", request2);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var errorContent = await secondResponse.Content.ReadAsStringAsync();

        errorContent.Should().Contain("Já existe um cupom com o código 'DUPLICATE'");
    }

    [Fact]
    public async Task CreateCoupon_EmptyCode_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateCouponRequest(string.Empty, DiscountType.Percentage, 10m); // Código vazio 

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCoupon_InvalidDiscountValue_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateCouponRequest("INVALID", DiscountType.Percentage, 0m); // Valor inválido

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCoupon_PercentageOver100_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateCouponRequest("INVALID_PERCENT", DiscountType.Percentage, 150m); // Mais de 100%

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Coupon Tests

    [Fact]
    public async Task GetCoupon_ExistingCoupon_ShouldReturnOk()
    {
        // Arrange
        var createRequest = new CreateCouponRequest("GETTEST", DiscountType.Fixed, 25m, MinimumOrderValue: 75m);

        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        // Act
        var response = await Client.GetAsync("/api/coupons/GETTEST");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var coupon = await response.Content.ReadFromJsonAsync<CouponResponse>();

        coupon.Should().NotBeNull();
        coupon.Code.Should().Be("GETTEST");
        coupon.DiscountValue.Should().Be(25m);
        coupon.MinimumOrderValue.Should().Be(75m);
    }

    [Fact]
    public async Task GetCoupon_NonExistingCoupon_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.GetAsync("/api/coupons/NONEXISTENT");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Cupom não encontrado");
    }

    [Fact]
    public async Task GetCoupon_CaseInsensitive_ShouldReturnOk()
    {
        // Arrange
        var createRequest = new CreateCouponRequest("CASETEST", DiscountType.Fixed, 10m);

        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        // Act - Buscar com case diferente
        var response = await Client.GetAsync("/api/coupons/casetest");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var coupon = await response.Content.ReadFromJsonAsync<CouponResponse>();
        coupon!.Code.Should().Be("CASETEST");
    }

    #endregion

    #region Get All Coupons Tests

    [Fact]
    public async Task GetAllCoupons_WithMultipleCoupons_ShouldReturnOkWithList()
    {
        // Arrange
        var request1 = new CreateCouponRequest("LIST1", DiscountType.Fixed, 10m);
        var request2 = new CreateCouponRequest("LIST2", DiscountType.Percentage, 15m);
        var request3 = new CreateCouponRequest("LIST3", DiscountType.Fixed, 20m);

        await Client.PostAsJsonAsync("/api/coupons", request1);
        await Client.PostAsJsonAsync("/api/coupons", request2);
        await Client.PostAsJsonAsync("/api/coupons", request3);

        // Act
        var response = await Client.GetAsync("/api/coupons");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var coupons = await response.Content.ReadFromJsonAsync<IEnumerable<CouponResponse>>();

        if (coupons != null)
        {
            var listCoupons = coupons.ToList();

            listCoupons.Should().NotBeNull();
            listCoupons.Should().Contain(c => c.Code == "LIST1");
            listCoupons.Should().Contain(c => c.Code == "LIST2");
            listCoupons.Should().Contain(c => c.Code == "LIST3");
        }
    }

    [Fact]
    public async Task GetAllCoupons_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Act
        var response = await Client.GetAsync("/api/coupons");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var coupons = await response.Content.ReadFromJsonAsync<IEnumerable<CouponResponse>>();

        if (coupons != null)
        {
            var listCoupons = coupons.ToList();

            listCoupons.Should().NotBeNull();
            listCoupons.Should().BeEmpty();
        }
    }

    #endregion

    #region Apply Coupon Tests

    [Fact]
    public async Task ApplyCoupon_ValidFixedDiscount_ShouldReturnSuccess()
    {
        // Arrange
        var createRequest = new CreateCouponRequest(
            "APPLY_FIXED",
            DiscountType.Fixed,
            30m,
            MinimumOrderValue: 100m);

        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        var applyRequest = new ApplyCouponRequest("APPLY_FIXED", "customer1", 150m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CouponApplicationResult>();

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(30m);
        result.FinalAmount.Should().Be(120m);
        result.ErrorMessage.Should().BeNull();
        result.ValidationErrors.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task ApplyCoupon_ValidPercentageDiscount_ShouldReturnSuccess()
    {
        // Arrange
        var createRequest = new CreateCouponRequest(
            "APPLY_PERCENT",
            DiscountType.Percentage,
            20m,
            MinimumOrderValue: 50m);

        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        var applyRequest = new ApplyCouponRequest("APPLY_PERCENT", "customer2", 100m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CouponApplicationResult>();

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(20m); // 20% de 100
        result.FinalAmount.Should().Be(80m);
    }

    [Fact]
    public async Task ApplyCoupon_BelowMinimumValue_ShouldReturnInvalid()
    {
        // Arrange
        var createRequest = new CreateCouponRequest(
            "MIN_VALUE_TEST",
            DiscountType.Fixed,
            25m,
            MinimumOrderValue: 100m);

        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        var applyRequest = new ApplyCouponRequest("MIN_VALUE_TEST", "customer3", 50m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CouponApplicationResult>();

        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Cupom inválido");
        result.ValidationErrors.Should().Contain("Valor mínimo do pedido deve ser R$ 100,00");
    }

    [Fact]
    public async Task ApplyCoupon_UniquePerCustomerAlreadyUsed_ShouldReturnInvalid()
    {
        // Arrange
        var createRequest = new CreateCouponRequest(
            "UNIQUE_TEST",
            DiscountType.Fixed,
            15m,
            IsUniquePerCustomer: true);

        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        var applyRequest = new ApplyCouponRequest("UNIQUE_TEST", "customer4", 100m);

        // Act - Primeira aplicação
        var firstResponse = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstResult = await firstResponse.Content.ReadFromJsonAsync<CouponApplicationResult>();
        firstResult!.IsValid.Should().BeTrue();

        // Act - Segunda aplicação pelo mesmo cliente
        var secondResponse = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResult = await secondResponse.Content.ReadFromJsonAsync<CouponApplicationResult>();

        secondResult.Should().NotBeNull();
        secondResult.IsValid.Should().BeFalse();
        secondResult.ErrorMessage.Should().Be("Cupom inválido");
        secondResult.ValidationErrors.Should().Contain("Este cupom já foi utilizado por você anteriormente");
    }

    [Fact]
    public async Task ApplyCoupon_UniquePerCustomerDifferentCustomers_ShouldReturnSuccess()
    {
        // Arrange
        var createRequest = new CreateCouponRequest(
            "UNIQUE_MULTI",
            DiscountType.Fixed,
            10m,
            IsUniquePerCustomer: true);

        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        var applyRequest1 = new ApplyCouponRequest("UNIQUE_MULTI", "customer5", 100m);
        var applyRequest2 = new ApplyCouponRequest("UNIQUE_MULTI", "customer6", 100m);

        // Act
        var response1 = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest1);
        var response2 = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var result1 = await response1.Content.ReadFromJsonAsync<CouponApplicationResult>();
        var result2 = await response2.Content.ReadFromJsonAsync<CouponApplicationResult>();

        result1!.IsValid.Should().BeTrue();
        result2!.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyCoupon_ExpiredCoupon_ShouldReturnInvalid()
    {
        // Arrange
        var createRequest = new CreateCouponRequest(
            "EXPIRED_TEST",
            DiscountType.Fixed,
            20m,
            ExpirationDate: DateTime.UtcNow.AddDays(-1)); // Expirado ontem

        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        var applyRequest = new ApplyCouponRequest("EXPIRED_TEST", "customer7", 100m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CouponApplicationResult>();

        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Cupom inválido");
        result.ValidationErrors.Should().ContainMatch("Cupom expirado em *");
    }

    [Fact]
    public async Task ApplyCoupon_NonExistentCoupon_ShouldReturnInvalid()
    {
        // Arrange
        var applyRequest = new ApplyCouponRequest("NONEXISTENT_COUPON", "customer8", 100m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CouponApplicationResult>();

        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Cupom não encontrado");
        result.DiscountAmount.Should().Be(0);
        result.FinalAmount.Should().Be(0);
    }

    [Fact]
    public async Task ApplyCoupon_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new ApplyCouponRequest(
            "", // Código vazio
            "customer9",
            100m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons/apply", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApplyCoupon_FixedDiscountGreaterThanOrderValue_ShouldLimitDiscount()
    {
        // Arrange
        var createRequest = new CreateCouponRequest(
            "BIG_DISCOUNT",
            DiscountType.Fixed,
            100m); // Desconto maior que o valor do pedido

        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        var applyRequest = new ApplyCouponRequest("BIG_DISCOUNT", "customer10", 50m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CouponApplicationResult>();

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(50m); // Limitado ao valor do pedido
        result.FinalAmount.Should().Be(0m);
    }

    #endregion

    #region Deactivate Coupon Tests

    [Fact]
    public async Task DeactivateCoupon_ExistingCoupon_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateCouponRequest("DEACTIVATE_TEST", DiscountType.Fixed, 10m);
        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        // Act
        var response = await Client.PatchAsync("/api/coupons/DEACTIVATE_TEST/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar se o cupom foi realmente desativado
        var getResponse = await Client.GetAsync("/api/coupons/DEACTIVATE_TEST");
        var coupon = await getResponse.Content.ReadFromJsonAsync<CouponResponse>();
        coupon!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateCoupon_NonExistentCoupon_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.PatchAsync("/api/coupons/NONEXISTENT_DEACTIVATE/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Cupom com código 'NONEXISTENT_DEACTIVATE' não encontrado");
    }

    [Fact]
    public async Task ApplyCoupon_InactiveCoupon_ShouldReturnInvalid()
    {
        // Arrange
        var createRequest = new CreateCouponRequest("INACTIVE_TEST", DiscountType.Fixed, 10m);
        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        // Desativar o cupom
        await Client.PatchAsync("/api/coupons/INACTIVE_TEST/deactivate", null);

        var applyRequest = new ApplyCouponRequest("INACTIVE_TEST", "customer11", 100m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CouponApplicationResult>();

        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Cupom inválido");
        result.ValidationErrors.Should().Contain("Cupom não está ativo");
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
        content.Should().Contain("timestamp");
    }

    #endregion

    #region Usage Tracking Tests

    [Fact]
    public async Task ApplyCoupon_ShouldTrackUsageCount()
    {
        // Arrange
        var createRequest = new CreateCouponRequest("USAGE_TRACK", DiscountType.Fixed, 5m, IsUniquePerCustomer: false); // Permite múltiplos usos

        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        // Act - Aplicar cupom 3 vezes com clientes diferentes
        await Client.PostAsJsonAsync("/api/coupons/apply", new ApplyCouponRequest("USAGE_TRACK", "customer12", 100m));
        await Client.PostAsJsonAsync("/api/coupons/apply", new ApplyCouponRequest("USAGE_TRACK", "customer13", 100m));
        await Client.PostAsJsonAsync("/api/coupons/apply", new ApplyCouponRequest("USAGE_TRACK", "customer14", 100m));

        // Assert
        var getResponse = await Client.GetAsync("/api/coupons/USAGE_TRACK");
        var coupon = await getResponse.Content.ReadFromJsonAsync<CouponResponse>();

        coupon.Should().NotBeNull();
        coupon.UsageCount.Should().Be(3);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ApplyCoupon_ZeroOrderValue_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new CreateCouponRequest("ZERO_ORDER", DiscountType.Fixed, 10m);
        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        var applyRequest = new ApplyCouponRequest("ZERO_ORDER", "customer15", 0m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCoupon_VeryLongCode_ShouldReturnBadRequest()
    {
        // Arrange
        var longCode = new string('A', 51); // 51 caracteres (máximo é 50)
        var request = new CreateCouponRequest(longCode, DiscountType.Fixed, 10m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApplyCoupon_EmptyCustomerId_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new CreateCouponRequest("EMPTY_CUSTOMER", DiscountType.Fixed, 10m);
        await Client.PostAsJsonAsync("/api/coupons", createRequest);

        var applyRequest = new ApplyCouponRequest("EMPTY_CUSTOMER", "", 100m);

        // Act
        var response = await Client.PostAsJsonAsync("/api/coupons/apply", applyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
