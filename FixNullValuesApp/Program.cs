using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Database.DatabaseContext;
using System;
using System.Threading.Tasks;

namespace FixNullValuesApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting database null values fix...");

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=(localdb)\\mssqllocaldb;Database=PharmacyManagementDB;Trusted_Connection=true;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<PharmacyContext>()
                .UseSqlServer(connectionString)
                .Options;

            try
            {
                using var context = new PharmacyContext(options);
                
                Console.WriteLine("Connected to database successfully.");
                Console.WriteLine("Fixing NULL PaymentStatus values...");
                
                var sql1 = "UPDATE Sales SET PaymentStatus = 'Paid' WHERE PaymentStatus IS NULL";
                var rowsAffected1 = await context.Database.ExecuteSqlRawAsync(sql1);
                Console.WriteLine($"Updated {rowsAffected1} rows with NULL PaymentStatus");

                Console.WriteLine("Fixing NULL Note values...");
                var sql2 = "UPDATE Sales SET Note = '' WHERE Note IS NULL";
                var rowsAffected2 = await context.Database.ExecuteSqlRawAsync(sql2);
                Console.WriteLine($"Updated {rowsAffected2} rows with NULL Note");

                Console.WriteLine("Database null values fix completed successfully!");
                Console.WriteLine($"Total rows updated: {rowsAffected1 + rowsAffected2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fixing database: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}