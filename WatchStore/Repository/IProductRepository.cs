using WatchStore.Models;

namespace WatchStore.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        IEnumerable<Product> SearchWatch(string searchTerm);
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task<bool> IsProductLikedByUser(int productId, int userId);
        Task AddProductLike(int productId, int userId);
        Task DeleteAsync(int id);
        Task<IEnumerable<Product>> GetLikedProductsByUserIdAsync(int userId);
    }
}

