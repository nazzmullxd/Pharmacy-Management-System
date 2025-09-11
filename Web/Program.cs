using Business;
using Database.Context;
using Database.Interfaces;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register the PharmacyManagementContext with the DI container
builder.Services.AddDbContext<PharmacyManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IProductBatchRepository, ProductBatchRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IPurchaseItemRepository, PurchaseItemRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleItemRepository, SaleItemRepository>();
builder.Services.AddScoped<IAntibioticLogRepository, AntibioticLogRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Register all business services
builder.Services.AddBusinessServices();
// Register services and repositories
builder.Services.AddScoped<Business.Interfaces.IProductService, Business.Services.ProductService>();
builder.Services.AddScoped<Business.Interfaces.IUserService, Business.Services.UserService>();
builder.Services.AddScoped<Business.Services.UserService>(); // Register concrete class
builder.Services.AddScoped<Database.Interfaces.IProductRepository, Database.Repositories.ProductRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();