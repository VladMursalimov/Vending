using Vending.Models.Entities;

namespace VendingMachine.Models.ViewModels
{
    /// <summary>
    /// Модель представления каталога.
    /// </summary>
    public class CatalogViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<string> Brands { get; set; } = new List<string>();
        public string SelectedBrand { get; set; } = "All";
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int CartCount { get; set; }
        public bool IsMachineLocked { get; set; }
    }

    /// <summary>
    /// Модель представления заказанных продуктов.
    /// </summary>
    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
    /// <summary>
    /// Модель представления заказа.
    /// </summary>
    public class OrderViewModel
    {
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }
    /// <summary>
    /// Модель представления заказа оплаты.
    /// </summary>
    public class PaymentViewModel
    {
        public decimal TotalAmount { get; set; }
        public List<Coin> Coins { get; set; } = new List<Coin>();
    }

    /// <summary>
    /// Модель запроса на добавление товара в заказ.
    /// </summary>
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

}