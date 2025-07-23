using backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
      public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
      {
            public DbSet<Category> Categories { get; set; }
            public DbSet<Product> Products { get; set; }
            public DbSet<ProductImage> ProductImages { get; set; }
            public DbSet<Order> Orders { get; set; }
            public DbSet<OrderItem> OrderItems { get; set; }
            public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }
            public DbSet<CommissionPayout> CommissionPayouts { get; set; }
            public DbSet<Cart> Carts { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);

                  // Category Table
                  modelBuilder.Entity<Category>(entity =>
            {
                  entity.ToTable("categories");
                  entity.HasKey(e => e.Id);
                  entity.Property(e => e.Name)
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");
            });

                  // Product Table
                  modelBuilder.Entity<Product>(entity =>
                  {
                        entity.ToTable("products");
                        entity.HasKey(e => e.Id);

                        entity.Property(e => e.Title)
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                        entity.Property(e => e.Description)
                        .HasColumnType("text");  // Changed from NVARCHAR(MAX)

                        entity.Property(e => e.Price)
                        .HasColumnType("decimal(10,2)");


                        entity.HasOne<Category>()
                        .WithMany()
                        .HasForeignKey(e => e.CategoryId)
                        .HasConstraintName("FK_products_categories");


                  });

                  // ProductImage Table
                  modelBuilder.Entity<ProductImage>(entity =>
                  {
                        entity.ToTable("product_images");
                        entity.HasKey(e => e.Id);

                        entity.Property(e => e.ImageUrl)
                        .IsRequired()
                        .HasColumnType("text"); // Changed from NVARCHAR(MAX)

                        entity.HasOne<Product>()
                        .WithMany()
                        .HasForeignKey(e => e.ProductId)
                        .HasConstraintName("FK_product_images_products");
                  });

                  // Order Table
                  modelBuilder.Entity<Order>(entity =>
                  {
                        entity.ToTable("orders");
                        entity.HasKey(e => e.Id);

                        entity.Property(e => e.OrderDate)
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                        entity.Property(e => e.TotalAmount)
                        .HasColumnType("decimal(10,2)");

                        entity.HasOne<User>()
                        .WithMany()
                        .HasForeignKey(e => e.UserId)
                        .HasConstraintName("FK_orders_users");
                  });

                  // OrderItem Table
                  modelBuilder.Entity<OrderItem>(entity =>
                  {
                        entity.ToTable("order_items");
                        entity.HasKey(e => e.Id);

                        entity.Property(e => e.Price)
                        .HasColumnType("decimal(10,2)");

                        entity.HasOne(e => e.Order)
                        .WithMany(o => o.Items)
                        .HasForeignKey(e => e.OrderId)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK_order_items_orders");

                        entity.HasOne(e => e.Product)
                        .WithMany()
                        .HasForeignKey(e => e.ProductId)
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("FK_order_items_products");
                  });
                  modelBuilder.Entity<WithdrawalRequest>(entity =>
                  {
                        entity.HasKey(w => w.Id);

                        // Relationship: Each WithdrawalRequest belongs to a User
                        entity.HasOne(w => w.User)
                        .WithMany() // If you have a User.WithdrawalRequests collection, use .WithMany(u => u.WithdrawalRequests)
                        .HasForeignKey(w => w.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                        // Required fields
                        entity.Property(w => w.UserId).IsRequired();
                        entity.Property(w => w.Amount).IsRequired();
                        entity.Property(w => w.RequestDate).IsRequired();

                        // Default value for Status
                        entity.Property(w => w.Status)
                                                      .HasMaxLength(32)
                        .HasDefaultValue("Pending");

                        // Optional fields (ProcessedDate, Remarks) - nothing special needed unless you want a max length on Remarks
                        entity.Property(w => w.Remarks).HasMaxLength(255); // Optional: set a max length for remarks
                  });
                  modelBuilder.Entity<CommissionPayout>(entity =>
                  {
                        entity.HasKey(cp => cp.Id);

                        // Relationship: Each CommissionPayout belongs to one User
                        entity.HasOne(cp => cp.User)
                              .WithMany() // or .WithMany(u => u.CommissionPayouts) if you add a collection to User
                              .HasForeignKey(cp => cp.UserId)
                              .OnDelete(DeleteBehavior.Cascade);

                        entity.Property(cp => cp.UserId).IsRequired();
                        entity.Property(cp => cp.Amount).IsRequired();
                        entity.Property(cp => cp.PayoutDate).IsRequired();

                        // Default value for Status
                        entity.Property(cp => cp.Status)
                              .HasMaxLength(32)   // Or 50, or 255 — just not unlimited!
                              .HasDefaultValue("Pending");


                        // Optional: limit length of remarks for DB storage efficiency
                        entity.Property(cp => cp.Remarks).HasMaxLength(255);
                  });

            }
      }
}
