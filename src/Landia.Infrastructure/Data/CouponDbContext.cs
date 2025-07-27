using Landia.Core.Entities;
using Landia.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Landia.Infrastructure.Data;

public class CouponDbContext : DbContext
{
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();

    public CouponDbContext(DbContextOptions<CouponDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CouponConfiguration());
        modelBuilder.ApplyConfiguration(new CouponUsageConfiguration());
    }
}