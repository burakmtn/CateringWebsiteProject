using CateringWebsite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CateringWebsite.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    public DbSet<MenuCustomizationOption> MenuCustomizationOptions => Set<MenuCustomizationOption>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.DisplayName).HasMaxLength(120);
            entity.Property(user => user.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        builder.Entity<MenuItem>(entity =>
        {
            entity.Property(item => item.Price).HasColumnType("decimal(18,2)");
            entity.Property(item => item.ImagePath).HasMaxLength(260);
            entity.Property(item => item.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(item => item.Caretaker)
                .WithMany()
                .HasForeignKey(item => item.CaretakerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MenuCustomizationOption>(entity =>
        {
            entity.Property(option => option.PriceChange).HasColumnType("decimal(18,2)");
            entity.Property(option => option.GroupName).HasMaxLength(80);
            entity.Property(option => option.Name).HasMaxLength(120);
            entity.Property(option => option.OptionType).HasMaxLength(40);
            entity.HasOne(option => option.MenuItem)
                .WithMany(item => item.CustomizationOptions)
                .HasForeignKey(option => option.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
