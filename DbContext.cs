using Microsoft.EntityFrameworkCore;
using Vending.Models.Entities;

/// <summary>
/// Контекст базы данных
/// </summary>
public class VendingDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Coin> Coins { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public VendingDbContext(DbContextOptions<VendingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.ProductName)
            .IsRequired();
        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.BrandName)
            .IsRequired();

        modelBuilder.Entity<Brand>().HasData(
            new Brand { Id = 1, Name = "Coca-Cola" },
            new Brand { Id = 2, Name = "Pepsi" },
            new Brand { Id = 3, Name = "Fanta" },
            new Brand { Id = 4, Name = "Sprite" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Coca-Cola Classic", Price = 50, Stock = 10, BrandId = 1 },
            new Product { Id = 2, Name = "Coca-Cola Zero", Price = 50, Stock = 8, BrandId = 1 },
            new Product { Id = 3, Name = "Pepsi Max", Price = 45, Stock = 12, BrandId = 2 },
            new Product { Id = 4, Name = "Pepsi Wild Cherry", Price = 45, Stock = 6, BrandId = 2 },
            new Product { Id = 5, Name = "Fanta Orange", Price = 40, Stock = 15, BrandId = 3 },
            new Product { Id = 6, Name = "Fanta Grape", Price = 40, Stock = 10, BrandId = 3 },
            new Product { Id = 7, Name = "Sprite", Price = 40, Stock = 20, BrandId = 4 },
            new Product { Id = 8, Name = "Sprite No Sugar", Price = 40, Stock = 18, BrandId = 4 }
        );

        modelBuilder.Entity<Coin>().HasData(
            new Coin { Id = 1, Denomination = 1, Quantity = 100 },
            new Coin { Id = 2, Denomination = 2, Quantity = 100 },
            new Coin { Id = 3, Denomination = 5, Quantity = 50 },
            new Coin { Id = 4, Denomination = 10, Quantity = 20 }
        );
    }
}