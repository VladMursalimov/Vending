using Vending.Models.Entities;

namespace Vending.Repositories
{
    /// <summary>
    /// Класс реализующий интерфейс монеты.
    /// </summary>
    public class CoinRepository : ICoinRepository
    {
        private readonly VendingDbContext _context;

        public CoinRepository(VendingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Coin>> GetAllCoins()
        {
            return _context.Coins.ToList();
        }

        public async Task UpdateCoinQuantity(int denomination, int quantity)
        {
            var coin = _context.Coins.FirstOrDefault(c => c.Denomination == denomination);
            if (coin != null)
            {
                coin.Quantity += quantity;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Coin>> GetCoinsDesc()
        {
            return _context.Coins.OrderByDescending(c => c.Denomination).ToList();
        }
    }
}
