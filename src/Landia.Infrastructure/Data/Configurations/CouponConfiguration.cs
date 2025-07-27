using Landia.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Landia.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.DiscountType).IsRequired().HasConversion<int>();

        builder.Property(c => c.DiscountValue).IsRequired().HasColumnType("decimal(18,2)");

        builder.Property(c => c.MinimumOrderValue).HasColumnType("decimal(18,2)");

        builder.Property(c => c.ExpirationDate).HasColumnType("datetime2");

        builder.Property(c => c.IsActive).IsRequired();

        builder.Property(c => c.IsUniquePerCustomer).IsRequired();

        builder.Property(c => c.CreatedAt).IsRequired().HasColumnType("datetime2");

        builder.Property(c => c.UpdatedAt).HasColumnType("datetime2");

        builder.HasMany(c => c.Usages).WithOne(u => u.Coupon).HasForeignKey(u => u.CouponId).OnDelete(DeleteBehavior.Cascade);

        // Configuração para propriedade privada _usages
        var metadata = builder.Metadata.FindNavigation(nameof(Coupon.Usages));

        if (metadata != null)
        {
            metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}