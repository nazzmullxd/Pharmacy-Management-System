using Microsoft.Data.SqlClient;

var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=PharmacyManagementSystem;Trusted_Connection=true;";

var sql = @"
-- Make ProductBatchID nullable in PurchaseItems table
ALTER TABLE [PurchaseItems] 
ALTER COLUMN [ProductBatchID] uniqueidentifier NULL;
";

try 
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();
    
    using var command = new SqlCommand(sql, connection);
    var rowsAffected = command.ExecuteNonQuery();
    
    Console.WriteLine($"Database schema updated successfully. Rows affected: {rowsAffected}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}