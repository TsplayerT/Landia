using Landia.Core.Entities;

namespace Landia.Core.Interfaces;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Coupon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Coupon> CreateAsync(Coupon coupon, CancellationToken cancellationToken = default);
    Task UpdateAsync(Coupon coupon, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);
}