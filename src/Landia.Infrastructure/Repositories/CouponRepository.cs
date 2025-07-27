using Landia.Core.Entities;
using Landia.Core.Interfaces;
using Landia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Landia.Infrastructure.Repositories;

public class CouponRepository : ICouponRepository
{
    private CouponDbContext Context { get; }
    private ILogger<CouponRepository> Logger { get; }

    public CouponRepository(CouponDbContext context, ILogger<CouponRepository> logger)
    {
        Context = context;
        Logger = logger;
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Buscando cupom por código: {CouponCode}", code);

        return await Context.Coupons.Include(c => c.Usages).FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task<Coupon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Buscando cupom por ID: {CouponId}", id);

        return await Context.Coupons.Include(c => c.Usages).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Buscando todos os cupons");

        return await Context.Coupons.Include(c => c.Usages).OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<Coupon> CreateAsync(Coupon coupon, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Criando cupom: {CouponId}", coupon.Id);

        Context.Coupons.Add(coupon);

        await Context.SaveChangesAsync(cancellationToken);

        return coupon;
    }

    public async Task UpdateAsync(Coupon coupon, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Atualizando cupom: {CouponId}", coupon.Id);

        Context.Coupons.Update(coupon);

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Verificando existência do cupom: {CouponCode}", code);

        return await Context.Coupons.AnyAsync(c => c.Code == code.ToUpperInvariant(), cancellationToken);
    }
}