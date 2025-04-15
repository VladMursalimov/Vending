using Vending.Models.Entities;

namespace Vending.Repositories
{

    /// <summary>
    /// Класс реализующий интерфейс заказа.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly VendingDbContext _context;

        public OrderRepository(VendingDbContext context)
        {
            _context = context;
        }

        public async Task SaveOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }
    }
}
