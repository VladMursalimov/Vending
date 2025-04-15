namespace Vending.Models.Entities
{
    /// <summary>
    /// Класс таблицы "Продукты".
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
    }

    /// <summary>
    /// Класс таблицы "Бренды".
    /// </summary>
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }

    /// <summary>
    /// Класс таблицы "Монеты".
    /// </summary>
    public class Coin
    {
        public int Id { get; set; }
        public int Denomination { get; set; }
        public int Quantity { get; set; }
    }
    /// <summary>
    /// Класс таблицы "Заказы".
    /// </summary>
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<OrderItem> Items { get; set; }
    }
    /// <summary>
    /// Класс таблицы "Заказанные товары".
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
