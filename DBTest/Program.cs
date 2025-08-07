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
        public void CanInsertAndRetrieveUserSupplierAndProduct()
        {
            var options = new DbContextOptionsBuilder<PharmacyManagementContext>()
                .UseSqlServer(@"Server=.;Database=Pharmacy Management System;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;

            using var context = new PharmacyManagementContext(options);

            // Arrange: create a new user
            var user = new User
            {
                UserID = Guid.NewGuid(),
                FirstName = "testuser",
                LastName = "test",
                Email = "testuser@example.com",
                PasswordHash = "hashedpassword",
                Role = "Admin",
                LastLoginDate= DateTime.UtcNow
            };
            context.Users.Add(user);

            // Arrange: create a new supplier
            var supplier = new Supplier
            {
                SupplierName = "Test Supplier",
                ContactPerson = "John Doe",
                PhoneNumber = "1234567890",
                Email = "supplier@example.com",
                Address = "123 Main St",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = "Admin",
                IsActive = true
            };
            context.Suppliers.Add(supplier);

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

            // Act: retrieve the user, supplier, and product
            var retrievedUser = context.Users.FirstOrDefault(u => u.UserId == user.UserId);
            var retrievedSupplier = context.Suppliers.FirstOrDefault(s => s.SupplierID == supplier.SupplierID);
            var retrievedProduct = context.Products.FirstOrDefault(p => p.ProductID == product.ProductID);

            // Assert: check if retrieved entities match
            Assert.NotNull(retrievedUser);
            Assert.Equal("testuser", retrievedUser.FirstName);

            Assert.NotNull(retrievedSupplier);
            Assert.Equal("Test Supplier", retrievedSupplier.SupplierName);

            Assert.NotNull(retrievedProduct);
            Assert.Equal("Test Product", retrievedProduct.ProductName);
        }
    }
}