using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WatchStore.Models;

public partial class WatchStoreContext : DbContext
{
    public WatchStoreContext()
    {
    }

    public WatchStoreContext(DbContextOptions<WatchStoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<LikedProduct> LikedProducts { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-POTNR7PE\\SQLEXPRESS;Database=WatchStore;Integrated Security=true;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<LikedProduct>(entity =>
        {
            entity.HasKey(e => e.LikedProductId).HasName("PK__LikedPro__8658FCB31FCDBCEB");

            entity.HasOne(d => d.Product).WithMany(p => p.LikedProducts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__LikedProd__Produ__21B6055D");

            entity.HasOne(d => d.User).WithMany(p => p.LikedProducts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__LikedProd__UserI__22AA2996");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_User");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetails_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails).HasForeignKey(d => d.ProductId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products).HasForeignKey(d => d.BrandId);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasMany(d => d.Products).WithMany(p => p.Images)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductProductImage",
                    r => r.HasOne<Product>().WithMany().HasForeignKey("ProductsId"),
                    l => l.HasOne<ProductImage>().WithMany().HasForeignKey("ImagesId"),
                    j =>
                    {
                        j.HasKey("ImagesId", "ProductsId");
                        j.ToTable("ProductProductImage");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
