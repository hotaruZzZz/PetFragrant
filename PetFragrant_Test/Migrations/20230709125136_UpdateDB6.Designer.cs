﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PetFragrant_Test.Data;

namespace PetFragrant_Test.Migrations
{
    [DbContext(typeof(PetContext))]
    [Migration("20230709125136_UpdateDB6")]
    partial class UpdateDB6
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.17")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PetFragrant_Test.Models.Category", b =>
                {
                    b.Property<string>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("CategoryID");

                    b.Property<string>("CategoryName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FatherCategoryId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("FatherCategoryID");

                    b.HasKey("CategoryId");

                    b.HasIndex(new[] { "FatherCategoryId" }, "IX_Categories_FatherCategoryID");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Coupon", b =>
                {
                    b.Property<string>("CouponId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("CouponID");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrderId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("OrderID");

                    b.Property<DateTime>("Period")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Rate")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("CouponId");

                    b.HasIndex(new[] { "OrderId" }, "IX_Coupons_OrderID");

                    b.ToTable("Coupons");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Customer", b =>
                {
                    b.Property<string>("CustomerId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("CustomerID");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Birthday")
                        .HasColumnType("datetime2");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("EmailConfirmed")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValueSql("(CONVERT([bit],(0)))");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("bit");

                    b.Property<string>("Level")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CustomerId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Discount", b =>
                {
                    b.Property<string>("DiscoutID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Description");

                    b.Property<string>("DiscountType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("DiscountValue")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("DiscoutName")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Name");

                    b.Property<DateTime>("Period")
                        .HasColumnType("datetime2");

                    b.HasKey("DiscoutID");

                    b.ToTable("Discounts");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.MyLike", b =>
                {
                    b.Property<string>("ProdcutId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("CustomerId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("CustomerID");

                    b.HasKey("ProdcutId", "CustomerId");

                    b.HasIndex(new[] { "CustomerId" }, "IX_MyLikes_CustomerID");

                    b.ToTable("MyLikes");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Order", b =>
                {
                    b.Property<string>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("OrderID");

                    b.Property<DateTime>("Arriiveddate")
                        .HasColumnType("datetime2");

                    b.Property<string>("CustomerId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("CustomerID");

                    b.Property<string>("Delivery")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Orderdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Payment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Shipdate")
                        .HasColumnType("datetime2");

                    b.HasKey("OrderId");

                    b.HasIndex(new[] { "CustomerId" }, "IX_Orders_CustomerID");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.OrderDetail", b =>
                {
                    b.Property<string>("ProdcutId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("OrderId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("OrderID");

                    b.Property<int>("Amount")
                        .HasColumnType("int");

                    b.Property<decimal>("QtDiscount")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("ProdcutId", "OrderId");

                    b.HasIndex(new[] { "OrderId" }, "IX_OrderDetails_OrderID");

                    b.ToTable("OrderDetails");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Product", b =>
                {
                    b.Property<string>("ProdcutId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("CategoriesId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("CategoriesID");

                    b.Property<int>("Inventory")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ProductDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ProdcutId");

                    b.HasIndex(new[] { "CategoriesId" }, "IX_Products_CategoriesID");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.ProductSpec", b =>
                {
                    b.Property<string>("ProdcutId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("SpecId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("SpecID");

                    b.HasKey("ProdcutId", "SpecId");

                    b.HasIndex(new[] { "SpecId" }, "IX_ProductSpecs_SpecID");

                    b.ToTable("ProductSpecs");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.ShoppingCart", b =>
                {
                    b.Property<string>("ProdcutId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("CustomerId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("CustomerID");

                    b.Property<string>("SpecId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("SpecID");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("ProdcutId", "CustomerId", "SpecId");

                    b.HasIndex(new[] { "CustomerId" }, "IX_shoppingCarts_CustomerID");

                    b.HasIndex(new[] { "SpecId" }, "IX_shoppingCarts_SpecID");

                    b.ToTable("shoppingCarts");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Spec", b =>
                {
                    b.Property<string>("SpecId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("SpecID");

                    b.Property<int>("Inventory")
                        .HasColumnType("int");

                    b.Property<string>("SpecName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SpecId");

                    b.ToTable("Specs");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Category", b =>
                {
                    b.HasOne("PetFragrant_Test.Models.Category", "FatherCategory")
                        .WithMany("InverseFatherCategory")
                        .HasForeignKey("FatherCategoryId");

                    b.Navigation("FatherCategory");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Coupon", b =>
                {
                    b.HasOne("PetFragrant_Test.Models.Order", "Order")
                        .WithMany("Coupons")
                        .HasForeignKey("OrderId");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.MyLike", b =>
                {
                    b.HasOne("PetFragrant_Test.Models.Customer", "Customer")
                        .WithMany("MyLikes")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PetFragrant_Test.Models.Product", "Prodcut")
                        .WithMany("MyLikes")
                        .HasForeignKey("ProdcutId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("Prodcut");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Order", b =>
                {
                    b.HasOne("PetFragrant_Test.Models.Customer", "Customer")
                        .WithMany("Orders")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.OrderDetail", b =>
                {
                    b.HasOne("PetFragrant_Test.Models.Order", "Order")
                        .WithMany("OrderDetails")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PetFragrant_Test.Models.Product", "Prodcut")
                        .WithMany("OrderDetails")
                        .HasForeignKey("ProdcutId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Prodcut");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Product", b =>
                {
                    b.HasOne("PetFragrant_Test.Models.Category", "Categories")
                        .WithMany("Products")
                        .HasForeignKey("CategoriesId");

                    b.Navigation("Categories");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.ProductSpec", b =>
                {
                    b.HasOne("PetFragrant_Test.Models.Product", "Product")
                        .WithMany("ProductSpecs")
                        .HasForeignKey("ProdcutId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PetFragrant_Test.Models.Spec", "Spec")
                        .WithMany("ProductSpecs")
                        .HasForeignKey("SpecId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("Spec");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.ShoppingCart", b =>
                {
                    b.HasOne("PetFragrant_Test.Models.Customer", "Customer")
                        .WithMany("ShoppingCarts")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PetFragrant_Test.Models.Product", "Prodcut")
                        .WithMany("ShoppingCarts")
                        .HasForeignKey("ProdcutId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PetFragrant_Test.Models.Spec", "Spec")
                        .WithMany("ShoppingCarts")
                        .HasForeignKey("SpecId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("Prodcut");

                    b.Navigation("Spec");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Category", b =>
                {
                    b.Navigation("InverseFatherCategory");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Customer", b =>
                {
                    b.Navigation("MyLikes");

                    b.Navigation("Orders");

                    b.Navigation("ShoppingCarts");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Order", b =>
                {
                    b.Navigation("Coupons");

                    b.Navigation("OrderDetails");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Product", b =>
                {
                    b.Navigation("MyLikes");

                    b.Navigation("OrderDetails");

                    b.Navigation("ProductSpecs");

                    b.Navigation("ShoppingCarts");
                });

            modelBuilder.Entity("PetFragrant_Test.Models.Spec", b =>
                {
                    b.Navigation("ProductSpecs");

                    b.Navigation("ShoppingCarts");
                });
#pragma warning restore 612, 618
        }
    }
}
