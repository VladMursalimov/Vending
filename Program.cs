using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Vending.Repositories;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ExcelPackage.License.SetNonCommercialPersonal("vlad");
        // Добавление сервисов
        builder.Services.AddDbContext<VendingDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddControllersWithViews();

        // Добавление сервисов сессии
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        // Добавление репозиториев
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<ICoinRepository, CoinRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();

        var app = builder.Build();

        // Конфигурация midleware
        app.UseStaticFiles();
        app.UseRouting();

        app.UseSession();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}