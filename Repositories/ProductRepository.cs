using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Vending.Models.Entities;

namespace Vending.Repositories
{
    /// <summary>
    /// Класс реализующий интерфейс продукта.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly VendingDbContext _context;

        public ProductRepository(VendingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetFilteredProductsAsync(string brand = "All", decimal minPrice = 0, decimal maxPrice = 1000)
        {
            var query = _context.Products.Include(p => p.Brand).AsQueryable();

            if (brand != "All")
                query = query.Where(p => p.Brand.Name == brand);

            return await query.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToListAsync();
        }

        public async Task<List<string>> GetAllBrands()
        {
            return _context.Brands.Select(b => b.Name).ToList();
        }

        public async Task<Product> GetProductById(int id)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdateProductStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Stock -= quantity;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<int, Product>> GetProductsDictionary()
        {
            return _context.Products.ToDictionary(p => p.Id, p => p);
        }

        public void ImportProducts(List<IFormFile> files)
        {
            if (files == null || !files.Any())
            {
                throw new ArgumentException("Отсутсвуют файлы.");
            }

            var file = files.First();
            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                throw new ArgumentException("Поддерживаются только .xlsx и .xls файлы.");
            }

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount < 2)
            {
                throw new ArgumentException("Файл пуст или в нем пустые строки.");
            }

            var products = new List<Product>();
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var name = worksheet.Cells[row, 1].Text?.Trim();
                    var priceText = worksheet.Cells[row, 2].Text?.Trim();
                    var stockText = worksheet.Cells[row, 3].Text?.Trim();
                    var brandIdText = worksheet.Cells[row, 4].Text?.Trim();

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(priceText) ||
                        string.IsNullOrEmpty(stockText) || string.IsNullOrEmpty(brandIdText))
                    {
                        Console.WriteLine($"Invalid data in row {row}: Name={name}, Price={priceText}, Stock={stockText}, BrandId={brandIdText}");
                        continue;
                    }

                    if (!decimal.TryParse(priceText, out var price) || price <= 0)
                    {
                        Console.WriteLine($"Invalid price in row {row}: {priceText}");
                        continue;
                    }

                    if (!int.TryParse(stockText, out var stock) || stock < 0)
                    {
                        Console.WriteLine($"Invalid stock in row {row}: {stockText}");
                        continue;
                    }

                    if (!int.TryParse(brandIdText, out var brandId) || brandId <= 0)
                    {
                        Console.WriteLine($"Invalid BrandId in row {row}: {brandIdText}");
                        continue;
                    }

                    var brand = _context.Brands.Find(brandId);
                    if (brand == null)
                    {
                        Console.WriteLine($"BrandId not found in row {row}: {brandId}");
                        continue;
                    }

                    products.Add(new Product
                    {
                        Name = name,
                        Price = price,
                        Stock = stock,
                        BrandId = brandId
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing row {row}: {ex.Message}");
                    continue;
                }
            }

            if (!products.Any())
            {
                throw new ArgumentException("Не найдено походящих напитков в файле " + file.FileName);
            }

            _context.Products.AddRange(products);
            _context.SaveChanges();
            Console.WriteLine($"Imported {products.Count} products");
        }

    }
}
