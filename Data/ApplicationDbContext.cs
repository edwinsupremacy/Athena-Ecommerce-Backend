using System;
using AthenaEcommerce_website.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AthenaEcommerce_website.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Item> Item { get; set; }
    public DbSet<ItemSize> ItemSize { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<OrderItem> OrderItem { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ItemSize>()
            .HasOne(s => s.Item)
            .WithMany(i => i.ItemSizes)
            .HasForeignKey(s => s.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Item)
            .WithMany(i => i.OrderItems)
            .HasForeignKey(oi => oi.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderReference)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.CheckoutRequestId)
            .IsUnique();

        modelBuilder.Entity<Item>()
            .Property(i => i.Price)
            .HasColumnType("numeric(18,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.PriceAtPurchase)
            .HasColumnType("numeric(18,2)");

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalPrice)
            .HasColumnType("numeric(18,2)");
    }
}