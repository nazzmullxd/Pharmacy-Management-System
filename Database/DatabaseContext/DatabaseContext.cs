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
                optionsBuilder.UseSqlServer(@"Server=localhost\MSSQLSERVER02;Database=Pharmacy Management System;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        public DbSet<UserInfo> UsersInfo { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductBatch> ProductBatches { get; set; }
        public DbSet<SaleItem> SalesItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AntibioticLog> AntibioticLogs { get; set; }
        public DbSet<Customer> Customers { get; set; } 



    }
}