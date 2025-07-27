using Landia.Core.DTOs;
using Landia.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Landia.Core.Services;

public class CouponService : ICouponService
{
    private ICouponRepository CouponRepository { get; }
    private ILogger<CouponService> Logger { get; }
    private IEnumerable<ICouponRuleValidator> Validators { get; }

    public CouponService(ICouponRepository couponRepository, IEnumerable<ICouponRuleValidator> validators, ILogger<CouponService> logger)
    {
        CouponRepository = couponRepository;
        Validators = validators;
        Logger = logger;
    }

    public async Task<CouponResponse> CreateCouponAsync(CreateCouponRequest request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Criando cupom com código {CouponCode}", request.Code);

        // Verificar se já existe um cupom com o mesmo código
        if (await CouponRepository.ExistsAsync(request.Code, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar cupom com código duplicado: {CouponCode}", request.Code);

            throw new InvalidOperationException($"Já existe um cupom com o código '{request.Code}'");
        }

        var coupon = request.ToCoupon();
        var createdCoupon = await CouponRepository.CreateAsync(coupon, cancellationToken);

        Logger.LogInformation("Cupom criado com sucesso: {CouponId}", createdCoupon.Id);

        return createdCoupon.ToResponse();
    }

    public async Task<CouponApplicationResult> ApplyCouponAsync(ApplyCouponRequest request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Aplicando cupom {CouponCode} para cliente {CustomerId} com valor {OrderValue:C}", request.CouponCode, request.CustomerId, request.OrderValue);

        var coupon = await CouponRepository.GetByCodeAsync(request.CouponCode, cancellationToken);

        if (coupon == null)
        {
            Logger.LogWarning("Cupom não encontrado: {CouponCode}", request.CouponCode);

            return CouponApplicationResult.Failure("Cupom não encontrado");
        }

        // Executar todas as validações
        var validationErrors = new List<string>();

        foreach (var validator in Validators)
        {
            var (isValid, errorMessage) = await validator.ValidateAsync(coupon, request.CustomerId, request.OrderValue, cancellationToken);

            if (!isValid && !string.IsNullOrEmpty(errorMessage))
            {
                validationErrors.Add(errorMessage);
            }
        }

        if (validationErrors.Count != 0)
        {
            Logger.LogWarning("Falha na validação do cupom {CouponCode}: {Errors}", request.CouponCode, string.Join(", ", validationErrors));

            return CouponApplicationResult.Failure("Cupom inválido", validationErrors);
        }

        // Calcular desconto
        var discountAmount = coupon.CalculateDiscount(request.OrderValue);
        var finalAmount = request.OrderValue - discountAmount;

        // Registrar uso do cupom
        coupon.AddUsage(request.CustomerId, request.OrderValue, discountAmount);
        await CouponRepository.UpdateAsync(coupon, cancellationToken);

        Logger.LogInformation("Cupom {CouponCode} aplicado com sucesso. Desconto: {Discount:C}, Valor final: {FinalAmount:C}", request.CouponCode, discountAmount, finalAmount);

        return CouponApplicationResult.Success(discountAmount, finalAmount);
    }

    public async Task<CouponResponse?> GetCouponAsync(string code, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Buscando cupom com código {CouponCode}", code);

        var coupon = await CouponRepository.GetByCodeAsync(code, cancellationToken);

        if (coupon != null)
        {
            return coupon.ToResponse();
        }

        return null;
    }

    public async Task<IEnumerable<CouponResponse>> GetAllCouponsAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Buscando todos os cupons");

        var coupons = await CouponRepository.GetAllAsync(cancellationToken);

        return coupons.Select(x => x.ToResponse());
    }

    public async Task DeactivateCouponAsync(string code, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Desativando cupom com código {CouponCode}", code);

        var coupon = await CouponRepository.GetByCodeAsync(code, cancellationToken);

        if (coupon == null)
        {
            Logger.LogWarning("Tentativa de desativar cupom inexistente: {CouponCode}", code);

            throw new InvalidOperationException($"Cupom com código '{code}' não encontrado");
        }

        coupon.Deactivate();
        await CouponRepository.UpdateAsync(coupon, cancellationToken);

        Logger.LogInformation("Cupom {CouponCode} desativado com sucesso", code);
    }
}