using Microsoft.EntityFrameworkCore;
using Database.Context;

// Create a temporary console app to fix the database schema
var optionsBuilder = new DbContextOptionsBuilder<PharmacyManagementContext>();
optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PharmacyManagementSystem;Trusted_Connection=true;");

using var context = new PharmacyManagementContext(optionsBuilder.Options);

try
{
    Console.WriteLine("Attempting to fix database schema...");
    
    // Execute the SQL to fix the schema
    var sql = @"
        -- Drop existing foreign key constraint if it exists
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PurchaseItems_ProductBatches_ProductBatchID')
        BEGIN
            ALTER TABLE [PurchaseItems] DROP CONSTRAINT [FK_PurchaseItems_ProductBatches_ProductBatchID];
        END
        
        -- Make ProductBatchID nullable
        ALTER TABLE [PurchaseItems] 
        ALTER COLUMN [ProductBatchID] uniqueidentifier NULL;
        
        -- Recreate foreign key constraint
        ALTER TABLE [PurchaseItems]
        ADD CONSTRAINT [FK_PurchaseItems_ProductBatches_ProductBatchID] 
        FOREIGN KEY ([ProductBatchID]) REFERENCES [ProductBatches]([ProductBatchID]);
    ";
    
    await context.Database.ExecuteSqlRawAsync(sql);
    Console.WriteLine("✅ Database schema fixed successfully!");
    Console.WriteLine("ProductBatchID in PurchaseItems table is now nullable.");
    Console.WriteLine("You can now create purchase orders without foreign key constraint errors.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error fixing database schema: {ex.Message}");
    Console.WriteLine("Please try running the SQL manually using SQL Server Management Studio.");
}