using Landia.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Landia.Infrastructure.Data.Configurations;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("CouponUsages");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.CouponId).IsRequired();

        builder.Property(u => u.CustomerId).IsRequired().HasMaxLength(100);

        builder.Property(u => u.OrderValue).IsRequired().HasColumnType("decimal(18,2)");

        builder.Property(u => u.DiscountApplied).IsRequired().HasColumnType("decimal(18,2)");

        builder.Property(u => u.UsedAt).IsRequired().HasColumnType("datetime2");

        builder.HasIndex(u => new { u.CouponId, u.CustomerId }).HasDatabaseName("IX_CouponUsage_CouponId_CustomerId");

        builder.HasIndex(u => u.UsedAt).HasDatabaseName("IX_CouponUsage_UsedAt");
    }
}