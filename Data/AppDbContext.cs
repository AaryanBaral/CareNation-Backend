using System.Linq.Expressions;
using backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
      public class AppDbContext : IdentityDbContext<User>
      {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

            public DbSet<Category> Categories { get; set; }
            public DbSet<Product> Products { get; set; }
            public DbSet<ProductImage> ProductImages { get; set; }
            public DbSet<BalanceTransfer> BalanceTransfers { get; set; }
            public DbSet<Vendor> Vendors { get; set; }
            public DbSet<Order> Orders { get; set; }
            public DbSet<ImpersonationTicket> ImpersonationTickets { get; set; } = default!;
            public DbSet<SystemCounter> SystemCounters { get; set; } = default!;
            public DbSet<OrderItem> OrderItems { get; set; }
            public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }
            public DbSet<CommissionPayout> CommissionPayouts { get; set; }
            public DbSet<Cart> Carts { get; set; }
            public DbSet<FundContribution> FundContributions { get; set; }
            public DbSet<TeamSalesProgress> TeamSalesProgresses { get; set; }
            public DbSet<RewardPayout> RewardPayouts { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);

                  // ---- GLOBAL SOFT-DELETE FILTER ----
                  // If an entity has a bool property named "IsDeleted", it will be auto-filtered out.
                  ApplyGlobalSoftDeleteFilter(modelBuilder);

                  // ---- Category ----
                  modelBuilder.Entity<Category>(entity =>
                  {
                        entity.ToTable("categories");
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.Name)
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");
                  });

                  // ---- FundContribution ----
                  modelBuilder.Entity<FundContribution>(e =>
                  {
                        e.ToTable("FundContributions");
                        e.HasKey(x => x.Id);

                        e.Property(x => x.UserId).IsRequired();
                        e.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
                        e.Property(x => x.ContributionDate).IsRequired();
                        e.Property(x => x.Remarks).HasMaxLength(512);

                        e.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                        e.HasIndex(x => new { x.UserId, x.ContributionDate });
                        e.HasIndex(x => new { x.UserId, x.Type, x.ContributionDate });
                  });

                  // ---- SystemCounter ----
                  modelBuilder.Entity<SystemCounter>(e =>
                  {
                        e.HasKey(x => x.Name);
                        e.Property(x => x.NextValue).IsRequired();
                  });

                  // ---- TeamSalesProgress ----
                  modelBuilder.Entity<TeamSalesProgress>(e =>
                  {
                        e.ToTable("TeamSalesProgress");
                        e.HasKey(x => x.UserId);

                        e.Property(x => x.LeftTeamSales).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                        e.Property(x => x.RightTeamSales).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                        e.Property(x => x.MatchedVolumeConsumed).HasColumnType("decimal(18,2)").HasDefaultValue(0);

                        e.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                  });

                  // ---- RewardPayout ----
                  modelBuilder.Entity<RewardPayout>(e =>
                  {
                        e.ToTable("RewardPayouts");
                        e.HasKey(x => x.Id);

                        e.Property(x => x.UserId).IsRequired();
                        e.Property(x => x.PayoutDate).IsRequired();
                        e.Property(x => x.MilestoneAmount).HasColumnType("decimal(18,2)").IsRequired();
                        e.Property(x => x.RankLabel).HasMaxLength(64).IsRequired();
                        e.Property(x => x.RewardItem).HasMaxLength(128);
                        e.Property(x => x.RoyaltyAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                        e.Property(x => x.TravelFundAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                        e.Property(x => x.CarFundAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                        e.Property(x => x.HouseFundAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);

                        e.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                        e.HasIndex(x => new { x.UserId, x.PayoutDate });
                        e.HasIndex(x => new { x.UserId, x.MilestoneAmount }).HasDatabaseName("IX_Reward_MilestoneOnce");
                  });

                  // ---- Product ----
                  modelBuilder.Entity<Product>(entity =>
                  {
                        entity.ToTable("products");
                        entity.HasKey(e => e.Id);

                        entity.Property(e => e.Title).IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
                        entity.Property(e => e.Description).HasColumnType("text");

                        entity.HasOne<Category>()
                        .WithMany()
                        .HasForeignKey(e => e.CategoryId)
                        .HasConstraintName("FK_products_categories");
                  });

                  // ---- ProductImage ----
                  modelBuilder.Entity<ProductImage>(entity =>
                  {
                        entity.ToTable("product_images");
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.ImageUrl).IsRequired().HasColumnType("text");

                        entity.HasOne<Product>()
                        .WithMany()
                        .HasForeignKey(e => e.ProductId)
                        .HasConstraintName("FK_product_images_products");
                  });

                  // ---- Order ----
                  modelBuilder.Entity<Order>(entity =>
                  {
                        entity.ToTable("orders");
                        entity.HasKey(e => e.Id);

                        entity.Property(e => e.OrderDate).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                        entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,2)");

                        entity.HasOne<User>()
                        .WithMany()
                        .HasForeignKey(e => e.UserId)
                        .HasConstraintName("FK_orders_users");
                  });

                  // ---- OrderItem ----
                  modelBuilder.Entity<OrderItem>(entity =>
                  {
                        entity.ToTable("order_items");
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.Price).HasColumnType("decimal(10,2)");

                        entity.HasOne(e => e.Order)
                        .WithMany(o => o.Items)
                        .HasForeignKey(e => e.OrderId)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK_order_items_orders");

                        // Keep RESTRICT: we will soft-delete Product instead of removing it.
                        entity.HasOne(e => e.Product)
                        .WithMany()
                        .HasForeignKey(e => e.ProductId)
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("FK_order_items_products");
                  });

                  // ---- WithdrawalRequest ----
                  modelBuilder.Entity<WithdrawalRequest>(entity =>
                  {
                        entity.HasKey(w => w.Id);

                        entity.HasOne(w => w.User)
                        .WithMany()
                        .HasForeignKey(w => w.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.Property(w => w.UserId).IsRequired();
                        entity.Property(w => w.Amount).IsRequired();
                        entity.Property(w => w.RequestDate).IsRequired();
                        entity.Property(w => w.Status).HasMaxLength(32).HasDefaultValue("Pending");
                        entity.Property(w => w.Remarks).HasMaxLength(255);
                  });

                  // ---- CommissionPayout ----
                  modelBuilder.Entity<CommissionPayout>(entity =>
                  {
                        entity.HasKey(cp => cp.Id);

                        entity.HasOne(cp => cp.User)
                        .WithMany()
                        .HasForeignKey(cp => cp.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.Property(cp => cp.UserId).IsRequired();
                        entity.Property(cp => cp.Amount).IsRequired();
                        entity.Property(cp => cp.PayoutDate).IsRequired();
                        entity.Property(cp => cp.Status).HasMaxLength(32).HasDefaultValue("Pending");
                        entity.Property(cp => cp.Remarks).HasMaxLength(255);
                  });

                  // ---- BalanceTransfer ----
                  modelBuilder.Entity<BalanceTransfer>()
                      .HasOne(t => t.Sender)
                      .WithMany()
                      .HasForeignKey(t => t.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                  modelBuilder.Entity<BalanceTransfer>()
                      .HasOne(t => t.Receiver)
                      .WithMany()
                      .HasForeignKey(t => t.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);
            }

            // applies: e => !EF.Property<bool>(e, "IsDeleted")
            private static void ApplyGlobalSoftDeleteFilter(ModelBuilder modelBuilder)
            {
                  foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                  {
                        var prop = entityType.FindProperty("IsDeleted");
                        if (prop == null || prop.ClrType != typeof(bool)) continue;

                        var param = Expression.Parameter(entityType.ClrType, "e");
                        var body = Expression.Equal(
                            Expression.Call(
                                typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(bool)),
                                param,
                                Expression.Constant("IsDeleted")),
                            Expression.Constant(false)
                        );
                        var lambda = Expression.Lambda(body, param);
                        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                  }
            }
      }
}
