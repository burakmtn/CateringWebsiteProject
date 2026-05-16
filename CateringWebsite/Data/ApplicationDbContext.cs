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

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OrderItemOption> OrderItemOptions => Set<OrderItemOption>();

    public DbSet<OrderItemReview> OrderItemReviews => Set<OrderItemReview>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.DisplayName).HasMaxLength(120);
            entity.Property(user => user.LocationAddress).HasMaxLength(260);
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

        builder.Entity<Order>(entity =>
        {
            entity.Property(order => order.DeliveryAddress).HasMaxLength(260);
            entity.Property(order => order.Status).HasMaxLength(40);
            entity.Property(order => order.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(order => order.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(order => order.User)
                .WithMany()
                .HasForeignKey(order => order.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(item => item.MenuName).HasMaxLength(120);
            entity.Property(item => item.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(item => item.Subtotal).HasColumnType("decimal(18,2)");
            entity.HasOne(item => item.MenuItem)
                .WithMany()
                .HasForeignKey(item => item.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Caretaker)
                .WithMany()
                .HasForeignKey(item => item.CaretakerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderItemOption>(entity =>
        {
            entity.Property(option => option.GroupName).HasMaxLength(80);
            entity.Property(option => option.Name).HasMaxLength(120);
            entity.Property(option => option.PriceChange).HasColumnType("decimal(18,2)");
            entity.HasOne(option => option.MenuCustomizationOption)
                .WithMany()
                .HasForeignKey(option => option.MenuCustomizationOptionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OrderItemReview>(entity =>
        {
            entity.Property(review => review.Comment).HasMaxLength(800);
            entity.Property(review => review.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(review => review.OrderItemId).IsUnique();
            entity.HasOne(review => review.OrderItem)
                .WithOne(item => item.Review)
                .HasForeignKey<OrderItemReview>(review => review.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(review => review.User)
                .WithMany()
                .HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(review => review.MenuItem)
                .WithMany()
                .HasForeignKey(review => review.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(review => review.Caretaker)
                .WithMany()
                .HasForeignKey(review => review.CaretakerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
