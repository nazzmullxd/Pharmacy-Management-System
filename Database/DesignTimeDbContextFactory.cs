using Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Database
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PharmacyManagementContext>
    {
        public PharmacyManagementContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PharmacyManagementContext>();
            optionsBuilder.UseSqlServer(@"Server=localhost\MSSQLSERVER02;Database=[Pharmacy Management System];Trusted_Connection=True;TrustServerCertificate=True;");

            return new PharmacyManagementContext(optionsBuilder.Options);
        }
    }
}