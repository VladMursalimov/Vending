using System.Text.Json;

namespace Vending.Models
{
    /// <summary>
    /// Класс с дополнительными методами расширения для работы с сессиями
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Сериализует объект в JSON и сохраняет в сессию по указанному ключу.
        /// </summary>
        /// <param name="session">Текущая сессия.</param>
        /// <param name="key">Ключ для сохранения в сессии.</param>
        /// <param name="value">Объект для сериализации и сохранения.</param>
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        /// <summary>
        /// Получает JSON строку из сессии по ключу и десериализует в указанный тип.
        /// </summary>
        /// <typeparam name="T">Тип, в который нужно десериализовать.</typeparam>
        /// <param name="session">Текущая сессия.</param>
        /// <param name="key">Ключ для получения данных из сессии.</param>
        /// <returns>
        /// Десериализованный объект или значение по умолчанию для типа T.
        /// </returns>
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }

    /// <summary>
    /// Класс корзины покупок.
    /// </summary>
    public class CartItem
    {
        // Идентификатор товара
        public int ProductId { get; set; }

        // Название товара
        public string Name { get; set; }

        // Количество товара в корзине
        public int Quantity { get; set; }

        // Цена за единицу товара
        public decimal Price { get; set; }
    }
}