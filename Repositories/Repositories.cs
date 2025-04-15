using Vending.Models.Entities;

namespace Vending.Repositories
{
    /// <summary>
    /// Интерфейс для репозитория продуктов.
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Метод для получения продуктов.
        /// </summary>
        /// <param name="brand">Бренд</param>
        /// <param name="minPrice">Минимальная цена</param>
        /// <param name="maxPrice">Максимальная цена</param>
        /// <returns>список продуктов</returns>
        Task<List<Product>> GetFilteredProductsAsync(string brand = "All", decimal minPrice = 0, decimal maxPrice = 1000);
        /// <summary>
        /// Метод для получения всех брендов.
        /// </summary>
        /// <returns>список брендов</returns>
        Task<List<string>> GetAllBrands();
        /// <summary>
        /// Метод лля получения продукта по его id.
        /// </summary>
        /// <param name="id">искомый id.</param>
        /// <returns>продукт</returns>
        Task<Product> GetProductById(int id);
        /// <summary>
        /// Метод для обновлнеия количества продукта.
        /// </summary>
        /// <param name="productId">id продукта</param>
        /// <param name="quantity">новое кол-во</param>
        /// <returns></returns>
        Task UpdateProductStockAsync(int productId, int quantity);
        /// <summary>
        /// Метод для получения продуктов в виде словаря.
        /// </summary>
        /// <returns>Словарь продуктов.</returns>
        Task<Dictionary<int, Product>> GetProductsDictionary();

        /// <summary>
        /// Импорт напитков из excel файла в таблицу Products.
        /// </summary>
        /// <param name="files">Файлы</param>
        void ImportProducts(List<IFormFile> files);

    }

    /// <summary>
    /// Интерфейс репозитория монет.
    /// </summary>
    public interface ICoinRepository
    {
        Task<List<Coin>> GetAllCoins();
        Task UpdateCoinQuantity(int denomination, int quantity);
        Task<List<Coin>> GetCoinsDesc();
    }

    /// <summary>
    /// Интерфейс репозитория заказа.
    /// </summary>
    public interface IOrderRepository
    {
        Task SaveOrderAsync(Order order);
    }

}
