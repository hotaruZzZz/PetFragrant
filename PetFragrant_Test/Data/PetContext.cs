using PetFragrant_Test.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Options;

namespace PetFragrant_Test.Data
{
    public partial class PetContext : DbContext
    {

        public PetContext(DbContextOptions<PetContext> options) : base(options) { }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Coupon> Coupons { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<MyLike> MyLikes { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductSpec> ProductSpecs { get; set; }
        public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public virtual DbSet<Spec> Specs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.FatherCategoryId, "IX_Categories_FatherCategoryID");

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.FatherCategoryId).HasColumnName("FatherCategoryID");

                entity.HasOne(d => d.FatherCategory)
                    .WithMany(p => p.InverseFatherCategory)
                    .HasForeignKey(d => d.FatherCategoryId);
            });

            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.HasIndex(e => e.OrderId, "IX_Coupons_OrderID");

                entity.Property(e => e.CouponId).HasColumnName("CouponID");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Coupons)
                    .HasForeignKey(d => d.OrderId);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.EmailConfirmed)
                    .IsRequired()
                    .HasDefaultValueSql("(CONVERT([bit],(0)))");
            });

            modelBuilder.Entity<MyLike>(entity =>
            {
                entity.HasKey(e => new { e.ProdcutId, e.CustomerId });

                entity.HasIndex(e => e.CustomerId, "IX_MyLikes_CustomerID");

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.MyLikes)
                    .HasForeignKey(d => d.CustomerId);

                entity.HasOne(d => d.Prodcut)
                    .WithMany(p => p.MyLikes)
                    .HasForeignKey(d => d.ProdcutId);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.CustomerId, "IX_Orders_CustomerID");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnName("CustomerID");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustomerId);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => new { e.ProdcutId, e.OrderId });

                entity.HasIndex(e => e.OrderId, "IX_OrderDetails_OrderID");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.QtDiscount).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId);

                entity.HasOne(d => d.Prodcut)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.ProdcutId);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProdcutId);

                entity.HasIndex(e => e.CategoriesId, "IX_Products_CategoriesID");

                entity.Property(e => e.CategoriesId).HasColumnName("CategoriesID");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Categories)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoriesId);
            });

            modelBuilder.Entity<ProductSpec>(entity =>
            {
                entity.HasKey(e => new { e.ProdcutId, e.SpecId });

                entity.HasIndex(e => e.SpecId, "IX_ProductSpecs_SpecID");

                entity.Property(e => e.SpecId).HasColumnName("SpecID");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductSpecs)
                    .HasForeignKey(d => d.ProdcutId);

                entity.HasOne(d => d.Spec)
                    .WithMany(p => p.ProductSpecs)
                    .HasForeignKey(d => d.SpecId);
            });

            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.HasKey(e => new { e.ProdcutId, e.CustomerId });

                entity.ToTable("shoppingCarts");

                entity.HasIndex(e => e.CustomerId, "IX_shoppingCarts_CustomerID");

                entity.HasIndex(e => e.SpecId, "IX_shoppingCarts_SpecID");

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.SpecId).HasColumnName("SpecID");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ShoppingCarts)
                    .HasForeignKey(d => d.CustomerId);

                entity.HasOne(d => d.Prodcut)
                    .WithMany(p => p.ShoppingCarts)
                    .HasForeignKey(d => d.ProdcutId);

                entity.HasOne(d => d.Spec)
                    .WithMany(p => p.ShoppingCarts)
                    .HasForeignKey(d => d.SpecId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Spec>(entity =>
            {
                entity.Property(e => e.SpecId).HasColumnName("SpecID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
