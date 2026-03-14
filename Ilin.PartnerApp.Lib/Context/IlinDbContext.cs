using Ilin.PartnerApp.Lib.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ilin.Partner.Lib.Context
{
    public class IlinDbContext : DbContext
    {
        public DbSet<PartnerType> PartnerTypes => Set<PartnerType>();
        public DbSet<PartnerApp.Lib.Entities.Partner> Partners => Set<PartnerApp.Lib.Entities.Partner>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<PartnerProductSale> PartnerProductSales => Set<PartnerProductSale>();

        public IlinDbContext()
        {
        }

        public IlinDbContext(DbContextOptions<IlinDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(
                    "Host=localhost;Port=5432;Database=ilin;Username=app;Password=123456789");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("app");

            ConfigurePartnerType(modelBuilder);
            ConfigurePartner(modelBuilder);
            ConfigureProduct(modelBuilder);
            ConfigurePartnerProductSale(modelBuilder);
        }

        private static void ConfigurePartnerType(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PartnerType>(entity =>
            {
                entity.ToTable("partner_types");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(x => x.Name)
                    .IsUnique();
            });
        }

        private static void ConfigurePartner(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PartnerApp.Lib.Entities.Partner>(entity =>
            {
                entity.ToTable("partners");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.TypeId)
                    .HasColumnName("type_id")
                    .IsRequired();

                entity.Property(x => x.CompanyName)
                    .HasColumnName("company_name")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.Address)
                    .HasColumnName("address")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.Inn)
                    .HasColumnName("inn")
                    .HasMaxLength(12);

                entity.Property(x => x.DirectorFullname)
                    .HasColumnName("director_fullname")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.Rating)
                    .HasColumnName("rating")
                    .IsRequired();

                entity.Property(x => x.LogoPath)
                    .HasColumnName("logo_path")
                    .HasMaxLength(500);

                entity.Property(x => x.SalesPlaces)
                    .HasColumnName("sales_places");

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();

                entity.HasOne(x => x.PartnerType)
                    .WithMany(x => x.Partners)
                    .HasForeignKey(x => x.TypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(x => x.PartnerProductSales)
                    .WithOne(x => x.Partner)
                    .HasForeignKey(x => x.PartnerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.TypeId);
                entity.HasIndex(x => x.CompanyName);
                entity.HasIndex(x => x.Email).IsUnique();
                entity.HasIndex(x => x.Inn).IsUnique();

                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("ck_partners_rating_non_negative", "rating >= 0");
                    table.HasCheckConstraint("ck_partners_inn_length", "inn IS NULL OR char_length(inn) IN (10, 12)");
                });
            });
        }

        private static void ConfigureProduct(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.Article)
                    .HasColumnName("article")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.ProductType)
                    .HasColumnName("product_type")
                    .HasMaxLength(100);

                entity.Property(x => x.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.Price)
                    .HasColumnName("price")
                    .HasPrecision(12, 2)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .HasColumnName("is_active")
                    .HasDefaultValue(true)
                    .IsRequired();

                entity.HasMany(x => x.PartnerProductSales)
                    .WithOne(x => x.Product)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.Article)
                    .IsUnique();

                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("ck_products_price_non_negative", "price >= 0");
                });
            });
        }

        private static void ConfigurePartnerProductSale(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PartnerProductSale>(entity =>
            {
                entity.ToTable("partner_product_sales");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(x => x.PartnerId)
                    .HasColumnName("partner_id")
                    .IsRequired();

                entity.Property(x => x.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                entity.Property(x => x.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired();

                entity.Property(x => x.SaleDate)
                    .HasColumnName("sale_date")
                    .IsRequired();

                entity.Property(x => x.UnitPrice)
                    .HasColumnName("unit_price")
                    .HasPrecision(12, 2)
                    .IsRequired();

                entity.Property(x => x.TotalAmount)
                    .HasColumnName("total_amount")
                    .HasPrecision(14, 2)
                    .HasComputedColumnSql("quantity * unit_price", stored: true);

                entity.Property(x => x.Comment)
                    .HasColumnName("comment");

                entity.HasOne(x => x.Partner)
                    .WithMany(x => x.PartnerProductSales)
                    .HasForeignKey(x => x.PartnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Product)
                    .WithMany(x => x.PartnerProductSales)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.PartnerId);
                entity.HasIndex(x => x.ProductId);
                entity.HasIndex(x => x.SaleDate);
                entity.HasIndex(x => new { x.PartnerId, x.SaleDate });

                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("ck_sales_quantity_positive", "quantity > 0");
                    table.HasCheckConstraint("ck_sales_unit_price_non_negative", "unit_price >= 0");
                });
            });
        }
    }
}