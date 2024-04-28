using WatchStore.Models;
using Microsoft.EntityFrameworkCore;

namespace WatchStore.Repository
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Include(p => p.Brand).ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetLikedProductsByUserIdAsync(int userId)
        {
            return await _context.LikedProducts
                .Where(lp => lp.UserId == userId)
                .Select(lp => lp.Product)
                .ToListAsync();
        }
        public async Task<bool> IsProductLikedByUser(int productId, int userId)
        {
            return await _context.LikedProducts.AnyAsync(pl => pl.ProductId == productId && pl.UserId == userId);
        }

        public async Task AddProductLike(int productId, int userId)
        {
            var productLike = new LikedProduct { ProductId = productId, UserId = userId };
            _context.LikedProducts.Add(productLike);
            await _context.SaveChangesAsync();
        }
        public IEnumerable<Product> SearchWatch(string searchTerm)
        {

            // Truy vấn các sản phẩm có tên chứa searchTerm từ cơ sở dữ liệu
            return _context.Products.Where(p => p.Name.Contains(searchTerm)).ToList();

        }
        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }
        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

        }
        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }


    }
}
