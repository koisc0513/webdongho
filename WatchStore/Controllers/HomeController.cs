using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WatchStore.Models;
using WatchStore.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
namespace WatchStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserDAO _userDAO;
        public HomeController(IProductRepository productRepository, IBrandRepository brandRepository,
            IHttpContextAccessor httpContextAccessor, UserDAO userDAO)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _httpContextAccessor = httpContextAccessor;
            _userDAO = userDAO;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Phương thức xử lý đăng ký
        [HttpPost]
        public IActionResult Register(User user)
        {
            // Kiểm tra xem người dùng đã tồn tại hay chưa
            if (_userDAO.GetUserByEmail(user.Email) != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View();
            }
            // Kiểm tra xem username và phone có để trống hay không
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                ModelState.AddModelError("UserName", "Username is required");
                return View();
            }

            if (string.IsNullOrWhiteSpace(user.Phone))
            {
                ModelState.AddModelError("Phone", "Phone is required");
                return View();
            }
            // Kiểm tra định dạng email
            if (!IsValidEmail(user.Email))
            {
                ModelState.AddModelError("Email", "Invalid email format");
                return View();
            }


            // Kiểm tra và gán giá trị mặc định cho IsAdmin và Role
            user.IsAdmin = user.IsAdmin ?? 0;
            user.Role = user.Role ?? "khách";

            // Thêm người dùng mới vào cơ sở dữ liệu
            _userDAO.AddUser(user);

            // Chuyển hướng đến trang đăng nhập hoặc trang chính
            return RedirectToAction("Login");
        }

        private bool IsValidEmail(string email)
        {
            // Sử dụng regular expression để kiểm tra định dạng email
            string pattern = @"^[A-Za-z0-9._%+-]+@gmail\.com$";
            return Regex.IsMatch(email, pattern);
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //Dang nhap
        [HttpPost]
        public IActionResult Login(User user)
        {
            User existingUser = _userDAO.GetUserByEmail(user.Email);

            if (existingUser == null || existingUser.Password != user.Password)
            {
                ModelState.AddModelError("LoginError", "Invalid email or password");
                return View();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, existingUser.UserId.ToString())
    };

            if (existingUser.IsAdmin == 1)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme))).Wait();
                return RedirectToAction("Index", "Admin");
            }

            HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme))).Wait();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Xóa session của giỏ hàng
            HttpContext.Session.Remove("Cart");

            // Xóa cookie của giỏ hàng
            Response.Cookies.Delete("Cart");
            HttpContext.SignOutAsync(); // Đăng xuất người dùng

            // Chuyển hướng đến trang chủ 
            return RedirectToAction("Index");
        }

        // Lấy thông tin user ID từ cookie
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
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            
            return View(products);
        }
        public async Task<IActionResult> SortByName()
        {
            var sortedProducts = await _productRepository.GetAllAsync();
            sortedProducts = sortedProducts.OrderBy(p => p.Name);
            return View("Index", sortedProducts);
        }
        public async Task<IActionResult> SortByNameDesc()
        {
            var sortedProducts = await _productRepository.GetAllAsync();
            sortedProducts = sortedProducts.OrderByDescending(p => p.Name);
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
        public IActionResult Search(string searchTerm, string sortOrder, int products)
        {
            // Gọi phương thức tìm kiếm từ repository
            IEnumerable<Product> searchResults = _productRepository.SearchWatch(searchTerm);

            // Trả về view với kết quả tìm kiếm
            return View("SearchResults", searchResults);
        }
       
        public async Task<IActionResult> Detail(int id)
        {
            ShoppingCart cartObj = new()
            {
                Product = await _productRepository.GetByIdAsync(id),
            };
            return View(cartObj);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
