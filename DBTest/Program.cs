    using System;
    using System.Linq;
    using Database.Context;
    using Database.Model;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    namespace Database.Tests
    {
        public class DBTest
        {
            [Fact]
            public void CanInsertAndRetrieveProduct()
            {
                var options = new DbContextOptionsBuilder<PharmacyManagementContext>()
                    .UseSqlServer(@"Server=.;Database=Pharmacy Management System;Trusted_Connection=True;TrustServerCertificate=True;")
                    .Options;

                using var context = new PharmacyManagementContext(options);
                // Arrange: create a new product
                var product = new Product
                {
                    ProductName = "Test Product",
                    GenericName = "Test Generic",
                    Manufacturer = "Test Manufacturer",
                    Category = "Test Category",
                    UnitPrice = 10.0m,
                    DefaultRetailPrice = 12.0m,
                    DefaultWholeSalePrice = 8.0m,
                    IsActive = true,
                    Barcode = "1234567890",
                    CreatedDate = DateTime.UtcNow
                };

                context.Products.Add(product);
                context.SaveChanges();

                // Act: retrieve the product
                var retrieved = context.Products.FirstOrDefault(p => p.ProductID == product.ProductID);

                // Assert: check if retrieved product matches
                Assert.NotNull(retrieved);
                Assert.Equal("Test Product", retrieved.ProductName);
            }
        }
    }