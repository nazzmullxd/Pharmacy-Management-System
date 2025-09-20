using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string connectionString = "Server=localhost\\MSSQLSERVER02;Database=PharmacyManagementSystem;Trusted_Connection=True;TrustServerCertificate=True;";
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("Connection successful!");
                Console.WriteLine("Connection string: " + connectionString);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }
}