using WatchStore.Models;

namespace WatchStore.Repository
{
    public interface IBrandRepository
    {

        Task<IEnumerable<Brand>> GetAllAsync();
        Task<Brand> GetAsync(int id);
        Task AddAsync(Brand brand);
        Task UpdateAsync(Brand brand);
        Task DeleteAsync(int id);
    }
}
