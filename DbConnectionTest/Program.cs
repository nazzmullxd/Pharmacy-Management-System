using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string connectionString = "Server=.;Database=PharmacyManagementSystem;Trusted_Connection=True;TrustServerCertificate=True;";
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("Connection successful!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }
}