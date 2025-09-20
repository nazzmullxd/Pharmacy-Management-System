using Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Database.Context
{
    public class PharmacyManagementContext : DbContext
    {
        public PharmacyManagementContext(DbContextOptions<PharmacyManagementContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=localhost\MSSQLSERVER02;Database=[Pharmacy Management System];Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        public DbSet<UserInfo> UsersInfo { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductBatch> ProductBatches { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AntibioticLog> AntibioticLogs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<StockAdjustment> StockAdjustments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity to table mappings to match the actual database schema
            modelBuilder.Entity<SaleItem>().ToTable("SalesItems");
            
            // Explicit column mappings for SaleItem to ensure correct column names
            modelBuilder.Entity<SaleItem>()
                .Property(si => si.SaleItemID)
                .HasColumnName("SaleItemID");
            
            modelBuilder.Entity<SaleItem>()
                .Property(si => si.SaleID)
                .HasColumnName("SaleID");
            
            modelBuilder.Entity<SaleItem>()
                .Property(si => si.ProductID)
                .HasColumnName("ProductID");
            
            modelBuilder.Entity<SaleItem>()
                .Property(si => si.ProductBatchID)
                .HasColumnName("ProductBatchID");
            
            modelBuilder.Entity<SaleItem>()
                .Property(si => si.Quantity)
                .HasColumnName("Quantity");
            
            modelBuilder.Entity<SaleItem>()
                .Property(si => si.UnitPrice)
                .HasColumnName("UnitPrice");
            
            modelBuilder.Entity<SaleItem>()
                .Property(si => si.Discount)
                .HasColumnName("Discount");
            
            modelBuilder.Entity<SaleItem>()
                .Property(si => si.BatchNumber)
                .HasColumnName("BatchNumber");
            
            modelBuilder.Entity<SaleItem>()
                .Property(si => si.CreatedDate)
                .HasColumnName("CreatedDate");
            
            // Configure foreign key relationships to avoid cascade conflicts
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.ProductBatch)
                .WithMany()
                .HasForeignKey(p => p.ProductBatchID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasMany(p => p.PurchaseItems)
                .WithOne(pi => pi.Purchase)
                .HasForeignKey(pi => pi.PurchaseID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Product)
                .WithMany()
                .HasForeignKey(pi => pi.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.ProductBatch)
                .WithMany()
                .HasForeignKey(pi => pi.ProductBatchID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasMany(s => s.SaleItems)
                .WithOne(si => si.Sale)
                .HasForeignKey(si => si.SaleID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.ProductBatch)
                .WithMany()
                .HasForeignKey(si => si.ProductBatchID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure AntibioticLog relationships to avoid cascade conflicts
            modelBuilder.Entity<AntibioticLog>()
                .HasOne<Sale>()
                .WithMany()
                .HasForeignKey(al => al.SaleID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AntibioticLog>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(al => al.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AntibioticLog>()
                .HasOne<ProductBatch>()
                .WithMany()
                .HasForeignKey(al => al.ProductBatchID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AntibioticLog>()
                .HasOne<Customer>()
                .WithMany()
                .HasForeignKey(al => al.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure AuditLog relationships (UserID is commented out in model)
            // modelBuilder.Entity<AuditLog>()
            //     .HasOne<UserInfo>()
            //     .WithMany()
            //     .HasForeignKey(al => al.UserID)
            //     .OnDelete(DeleteBehavior.Restrict);

            // Configure StockAdjustment relationships  
            modelBuilder.Entity<StockAdjustment>()
                .HasOne(sa => sa.ProductBatch)
                .WithMany()
                .HasForeignKey(sa => sa.ProductBatchID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockAdjustment>()
                .HasOne<UserInfo>()
                .WithMany()
                .HasForeignKey(sa => sa.UserID)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure decimal properties with proper precision and scale
            modelBuilder.Entity<Product>()
                .Property(p => p.DefaultRetailPrice)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<Product>()
                .Property(p => p.DefaultWholeSalePrice)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<Purchase>()
                .Property(p => p.DueAmount)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<Purchase>()
                .Property(p => p.PaidAmount)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<Purchase>()
                .Property(p => p.TotalAmount)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<PurchaseItem>()
                .Property(p => p.UnitPrice)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<Sale>()
                .Property(s => s.TotalAmount)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<SaleItem>()
                .Property(s => s.Discount)
                .HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<SaleItem>()
                .Property(s => s.UnitPrice)
                .HasColumnType("decimal(18,2)");
        }
    }
}