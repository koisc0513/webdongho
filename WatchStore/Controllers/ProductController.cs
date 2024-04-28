using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WatchStore.Models;
using WatchStore.Repository;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
namespace WatchStore.Controllers
{
   
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductController(IProductRepository productRepository, IBrandRepository brandRepository, IHttpContextAccessor httpContextAccessor)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        private ApplicationDbContext db = new ApplicationDbContext();
        private const string connectionString = "LAPTOP-POTNR7PE\\SQLEXPRESS";

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            
            return View(products);
        }


        public IActionResult Search(string searchTerm, string sortOrder ,int products)
        {
            // Gọi phương thức tìm kiếm từ repository
            IEnumerable<Product> searchResults = _productRepository.SearchWatch(searchTerm);

            // Trả về view với kết quả tìm kiếm
            return View("SearchResults", searchResults);
        }
        private int GetUserId()
        {
            if (_httpContextAccessor.HttpContext.User.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
            {

                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }

            return 0;
        }
        public async Task<IActionResult> Like(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product != null)
            {
                // Xác định người dùng hiện tại
                var userId = GetUserId();
                if (userId != 0)
                {
                    // Kiểm tra xem người dùng đã thích sản phẩm này chưa
                    bool isLiked = await _productRepository.IsProductLikedByUser(productId, userId);
                    if (!isLiked)
                    {
                        // Nếu chưa thích, thêm vào bảng LikedProducts
                        await _productRepository.AddProductLike(productId, userId);
                    }
                    else
                    {
                        // Nếu đã thích, có thể xử lý việc hủy thích sản phẩm ở đây
                        // await _productRepository.RemoveProductLike(productId, userId);
                    }
                }
            }
            return RedirectToAction(nameof(Index)); // Chuyển hướng trở lại trang chính
        }

        public async Task<IActionResult> LikedProducts()
        {
            // Xác định người dùng hiện tại
            var userId = GetUserId();
            if (userId != 0)
            {
                var likedProducts = await _productRepository.GetLikedProductsByUserIdAsync(userId);
                return View(likedProducts);
            }
            return RedirectToAction("Login", "Home"); // Chuyển hướng nếu người dùng chưa đăng nhập
        }

        //Show add new book page
        public async Task<IActionResult> Add()
        {
            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Brand = new SelectList(brands, "Id", "Name");
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> Add(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productRepository.AddAsync(product);
                return RedirectToAction("Index");
            }
            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Brand = new SelectList(brands, "Id", "Name");
            return View(product);

        }

        //Show detail of a book
        public async Task<ActionResult> Detail(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //Show update page of the book
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Brands = new SelectList(brands, "Id", "Name", product);
            return View(product);

        }

        public async Task<IActionResult> SortByName()
        {
            var sortedProducts = await _productRepository.GetAllAsync();
            sortedProducts = sortedProducts.OrderBy(p => p.Name);
            return View("Index", sortedProducts);
        }

        public async Task<IActionResult> SortByPriceAsc()
        {
            var sortedProducts = await _productRepository.GetAllAsync();
            sortedProducts = sortedProducts.OrderBy(p => p.Price);
            return View("Index", sortedProducts);
        }
        public async Task<IActionResult> SortByPriceDesc()
        {
            var sortedProducts = await _productRepository.GetAllAsync();
            sortedProducts = sortedProducts.OrderByDescending(p => p.Price);
            return View("Index", sortedProducts);
        }

        //Process the book update
        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _productRepository.UpdateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        //Show the book delete confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();
            return View(product);
        }



        //Process the book deletion
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
