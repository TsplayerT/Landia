using Landia.Core;
using Landia.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Landia.Infrastructure.Data;

public class Seeder
{
    public CouponDbContext Context { get; }

    public Seeder(CouponDbContext context)
    {
        Context = context;
    }

    public async Task GenerateCuponsAsync()
    {
        var hasCoupons = await Context.Coupons.AnyAsync();

        if (hasCoupons)
        {
            return;
        }

        var coupons = new[]
        {
            new Coupon("WELCOME10", Enums.DiscountType.Percentage, 10m, 50m, isUniquePerCustomer: true),
            new Coupon("SAVE20", Enums.DiscountType.Fixed, 20m, 100m, DateTime.UtcNow.AddDays(30)),
            new Coupon("FLASH15", Enums.DiscountType.Percentage, 15m, expirationDate: DateTime.UtcNow.AddDays(7))
        };

        Context.Coupons.AddRange(coupons);

        await Context.SaveChangesAsync();
    }
}