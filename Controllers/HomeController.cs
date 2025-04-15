using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Vending.Models;
using Vending.Models.Entities;
using Vending.Repositories;
using VendingMachine.Models.ViewModels;

/// <summary>
/// Контроллер для бизнес-логики всего пути от заказа до оплаты. 
/// Использует три репозитория.
/// </summary>
public class HomeController : Controller
{
    // Репозиторий для работы с продуктами
    private readonly IProductRepository _productRepository;
    // Репозиторий для работы с монетами
    private readonly ICoinRepository _coinRepository;
    // Репозиторий для работы с заказами
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="productRepository"></param>
    /// <param name="coinRepository"></param>
    /// <param name="orderRepository"></param>
    public HomeController(IProductRepository productRepository, ICoinRepository coinRepository, IOrderRepository orderRepository)
    {
        _productRepository = productRepository;
        _coinRepository = coinRepository;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// GET Метод для обработки страницы каталога.
    /// </summary>
    /// <param name="brand">Бренд</param>
    /// <param name="minPrice">Минимальная цена</param>
    /// <param name="maxPrice">Максимальная цена</param>
    /// <returns>Страница каталога</returns>
    [HttpGet]
    public async Task<IActionResult> Index(string brand = "All", decimal minPrice = 0, decimal maxPrice = 1000)
    {
        var products = await _productRepository.GetFilteredProductsAsync(brand, minPrice, maxPrice);
        var brands = await _productRepository.GetAllBrands();

        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        var viewModel = new CatalogViewModel
        {
            Products = products,
            Brands = brands,
            SelectedBrand = brand,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            CartCount = cart.Sum(c => c.Quantity)
        };

        if (HttpContext.Session.GetString("MachineLocked") == "true" && !HttpContext.Session.GetString("SessionId").Equals(HttpContext.Session.Id))
        {
            viewModel.IsMachineLocked = true;
        }
        else
        {
            HttpContext.Session.SetString("MachineLocked", "true");
            HttpContext.Session.SetString("SessionId", HttpContext.Session.Id);
        }

        return View(viewModel);
    }


    /// <summary>
    /// GET Метод для обработки страницы заказа.
    /// </summary>
    /// <returns>Страница заказа.</returns>
    [HttpGet]
    public async Task<IActionResult> Order()
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        var products = await _productRepository.GetProductsDictionary();

        var viewModel = new OrderViewModel
        {
            Items = cart.Select(c => new OrderItemViewModel
            {
                ProductId = c.ProductId,
                Name = c.Name,
                Quantity = c.Quantity,
                Price = c.Price,
                Stock = products[c.ProductId].Stock
            }).ToList()
        };
        return View(viewModel);
    }

    /// <summary>
    /// POST Метод для создания/обновления заказа.
    /// </summary>
    /// <param name="items">Товары для заказа.</param>
    /// <returns>json {true/false}.</returns>
    [HttpPost]
    public async Task<IActionResult> UpdateOrder([FromBody] List<OrderItemViewModel> items)
    {
        Console.WriteLine($"UpdateOrder called with {items.Count} items");
        var cart = new List<CartItem>();

        foreach (var item in items)
        {
            var product = await _productRepository.GetProductById(item.ProductId);
            if (product == null)
            {
                Console.WriteLine($"Product not found: {item.ProductId}");
                return Json(new { success = false, message = $"Товар {item.Name} не найден" });
            }
            if (item.Quantity > product.Stock)
            {
                Console.WriteLine($"Stock limit exceeded for productId: {item.ProductId}, Stock: {product.Stock}, Requested: {item.Quantity}");
                return Json(new { success = false, message = $"Нельзя заказать больше {product.Stock} шт. {item.Name}" });
            }
            if (item.Quantity < 0)
            {
                Console.WriteLine($"Invalid quantity for productId: {item.ProductId}, Quantity: {item.Quantity}");
                return Json(new { success = false, message = $"Количество не может быть отрицательным" });
            }

            cart.Add(new CartItem
            {
                ProductId = item.ProductId,
                Name = item.Name,
                Quantity = item.Quantity,
                Price = item.Price
            });
        }

        HttpContext.Session.SetObjectAsJson("Cart", cart);
        Console.WriteLine($"Cart updated. Total items: {cart.Sum(c => c.Quantity)}");
        return Json(new { success = true });
    }
    /// <summary>
    /// GET Метод для обработки страницы оплаты.
    /// </summary>
    /// <returns>Страница оплаты.</returns>
    [HttpGet]
    public async Task<IActionResult> Payment()
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        Console.WriteLine($"Payment page loaded. Cart: {JsonSerializer.Serialize(cart)}");

        foreach (var item in cart)
        {
            var product = await _productRepository.GetProductById(item.ProductId);
            if (product == null || item.Price <= 0 || string.IsNullOrEmpty(item.Name))
            {
                Console.WriteLine($"Invalid cart item: ProductId={item.ProductId}, Price={item.Price}, Name={item.Name}");
                if (product != null)
                {
                    item.Price = product.Price;
                    item.Name = product.Name;
                }
                else
                {
                    cart.Remove(item);
                }
            }
        }

        var viewModel = new PaymentViewModel
        {
            TotalAmount = cart.Sum(c => c.Quantity * c.Price),
            Coins = await _coinRepository.GetAllCoins()
        };

        HttpContext.Session.SetObjectAsJson("Cart", cart); // Update session with fixed cart
        Console.WriteLine($"PaymentViewModel: TotalAmount={viewModel.TotalAmount}");
        return View(viewModel);
    }

    /// <summary>
    /// POST Метод для добавления товаров в заказ.
    /// </summary>
    /// <param name="request">Товар и его количество.</param>
    /// <returns>json {true/false, кол-во товаров в заказе.}</returns>
    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        Console.WriteLine($"AddToCart called with productId: {request.ProductId}, quantity: {request.Quantity}");
        var product = await _productRepository.GetProductById(request.ProductId);
        if (product == null || product.Stock < request.Quantity)
        {
            Console.WriteLine($"Product not found or insufficient stock for productId: {request.ProductId}");
            return Json(new { success = false });
        }

        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        var item = cart.FirstOrDefault(c => c.ProductId == request.ProductId);

        int newQuantity = (item?.Quantity ?? 0) + request.Quantity;
        if (newQuantity > product.Stock)
        {
            Console.WriteLine($"Stock limit exceeded for productId: {request.ProductId}, Stock: {product.Stock}, Requested: {newQuantity}");
            return Json(new { success = false, message = $"Нельзя добавить больше {product.Stock} шт. {product.Name}" });
        }

        if (item != null)
            item.Quantity += request.Quantity;
        else
            cart.Add(new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Price = product.Price,
                Name = product.Name
            });

        HttpContext.Session.SetObjectAsJson("Cart", cart);
        Console.WriteLine($"Cart updated. Total items: {cart.Sum(c => c.Quantity)}");
        return Json(new { success = true, cartCount = cart.Sum(c => c.Quantity) });
    }

    /// <summary>
    /// POST Метод для обработки оплаты
    /// </summary>
    /// <param name="coinsInserted">Монеты для оплаты.</param>
    /// <returns>json {true/false, сдача.}</returns>
    [HttpPost]
    public async Task<IActionResult> ProcessPayment([FromBody] Dictionary<int, int> coinsInserted)
    {
        Console.WriteLine($"ProcessPayment called with coins: {JsonSerializer.Serialize(coinsInserted)}");

        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
        if (cart == null || !cart.Any())
        {
            Console.WriteLine("Cart is empty");
            return Json(new { success = false, message = "Заказ пуст" });
        }

        decimal totalAmount = cart.Sum(c => c.Quantity * c.Price);
        decimal insertedAmount = coinsInserted.Sum(c => c.Key * c.Value);

        Console.WriteLine($"Total amount: {totalAmount}, Inserted: {insertedAmount}");

        if (insertedAmount < totalAmount)
        {
            Console.WriteLine("Insufficient funds");
            return Json(new { success = false, message = "Недостаточно средств" });
        }

        decimal change = insertedAmount - totalAmount;
        var availableCoins = await _coinRepository.GetCoinsDesc();
        var changeCoins = new Dictionary<int, int>();

        foreach (var coin in availableCoins)
        {
            int count = (int)(change / coin.Denomination);
            if (count > coin.Quantity)
                count = coin.Quantity;
            if (count > 0)
            {
                changeCoins[coin.Denomination] = count;
                change -= count * coin.Denomination;
                await _coinRepository.UpdateCoinQuantity(coin.Denomination, -count);
            }
        }

        if (change > 0)
        {
            Console.WriteLine("Cannot provide exact change");
            return Json(new { success = false, message = "Невозможно вернуть сдачу" });
        }


        var order = new Order
        {
            OrderDate = DateTime.Now,
            TotalAmount = totalAmount,
            Items = cart.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                ProductName = c.Name,
                BrandName = (_productRepository.GetProductById(c.ProductId)).Result.Brand.Name,
                Quantity = c.Quantity,
                Price = c.Price
            }).ToList()
        };

        foreach (var item in cart)
        {
            await _productRepository.UpdateProductStockAsync(item.ProductId, item.Quantity);
        }

        foreach (var coin in coinsInserted)
        {
            await _coinRepository.UpdateCoinQuantity(coin.Key, coin.Value);
        }

        await _orderRepository.SaveOrderAsync(order);

        HttpContext.Session.Remove("Cart");
        HttpContext.Session.SetString("MachineLocked", "false");

        Console.WriteLine("Payment successful, change: " + JsonSerializer.Serialize(changeCoins));
        return Json(new { success = true, change = changeCoins });
    }

    /// <summary>
    /// POSt Метод для импорта напитков из файла.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult UploadProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            Console.WriteLine("No file uploaded");
            return Json(new { success = false, message = "Пожалуйста, выберите файл" });
        }

        try
        {
            _productRepository.ImportProducts(new List<IFormFile> { file });
            Console.WriteLine("Products imported successfully");
            return Json(new { success = true, message = "Напитки успешно загружены" });
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Invalid input: {ex.Message}");
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing products: {ex.Message}");
            return Json(new { success = false, message = "Ошибка при загрузке: " + ex.Message });
        }
    }
}