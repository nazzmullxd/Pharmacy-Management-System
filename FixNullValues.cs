using Database.Context;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Fixing NULL values in Sales table...");

var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=PharmacyManagementSystem;Trusted_Connection=true;TrustServerCertificate=true;";

var options = new DbContextOptionsBuilder<PharmacyManagementContext>()
    .UseSqlServer(connectionString)
    .Options;

using var context = new PharmacyManagementContext(options);

try
{
    // Update NULL PaymentStatus values
    await context.Database.ExecuteSqlRawAsync("UPDATE Sales SET PaymentStatus = 'Paid' WHERE PaymentStatus IS NULL");
    
    // Update NULL Note values
    await context.Database.ExecuteSqlRawAsync("UPDATE Sales SET Note = '' WHERE Note IS NULL");
    
    Console.WriteLine("Successfully updated NULL values in Sales table.");
    Console.WriteLine("PaymentStatus: NULL -> 'Paid'");
    Console.WriteLine("Note: NULL -> ''");
}
catch (Exception ex)
{
    Console.WriteLine($"Error fixing NULL values: {ex.Message}");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();