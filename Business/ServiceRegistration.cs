using Business.Interfaces;
using Business.Services;
using Database.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Business
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // Register core business services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISalesService, SalesService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ICustomerService, CustomerService>();
            
            // Register additional feature services
            services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ISupportTicketService, SupportTicketService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

            return services;
        }
    }
}

