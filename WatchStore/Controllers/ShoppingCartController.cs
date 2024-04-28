using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WatchStore.Models;
using WatchStore.Repository;

namespace WatchStore.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ShoppingCartController(IProductRepository productRepository, 
            ApplicationDbContext context, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _productRepository = productRepository;
            _context = context;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            // Lấy thông tin giỏ hàng từ cookie
            var cartJson = Request.Cookies["Cart"];
            var cart = cartJson != null ? JsonConvert.DeserializeObject<ShoppingCart>(cartJson) : new ShoppingCart();
            if (cart.Items.Count == 0)
            {
                return View("Empty");
            }
            return View(cart);
        }
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            // Lấy thông tin giỏ hàng từ cookie
            var cartJson = Request.Cookies["Cart"];
            if (string.IsNullOrEmpty(cartJson))
            {
                return RedirectToAction("Index");
            }

            var cart = JsonConvert.DeserializeObject<ShoppingCart>(cartJson);
            var itemToUpdate = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (itemToUpdate != null)
            {
                itemToUpdate.Quantity = quantity;
            }

            // Cập nhật lại cookie
            Response.Cookies.Append("Cart", JsonConvert.SerializeObject(cart));

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await GetProductFromDatabase(productId);
            if (product == null)
            {
                // Xử lý khi sản phẩm không tồn tại...
                return RedirectToAction("Index");
            }

            var cartItem = new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity,
            };

            // Lấy thông tin giỏ hàng từ cookie
            var cartJson = Request.Cookies["Cart"];
            var cart = cartJson != null ? JsonConvert.DeserializeObject<ShoppingCart>(cartJson) : new ShoppingCart();

            // Thêm sản phẩm vào giỏ hàng
            cart.Items.Add(cartItem);

            // Lưu giỏ hàng vào cookie
            Response.Cookies.Append("Cart", JsonConvert.SerializeObject(cart));

            return RedirectToAction("Index");
        }

        private async Task<Product> GetProductFromDatabase(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            return await _productRepository.GetByIdAsync(productId);
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
        public async Task<IActionResult> Checkout()
        {
            return View(new Order());
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            // nhận userid hiện tại 
            int userId = GetUserId();
            // Lấy thông tin giỏ hàng từ cookie
            var cartJson = Request.Cookies["Cart"];
            var cart = cartJson != null ? JsonConvert.DeserializeObject<ShoppingCart>(cartJson) : new ShoppingCart();
            if (cart == null)
            {
                // Xử lý giỏ hàng trống...
                return RedirectToAction("Index");
            }

           
            order.UserId = userId;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();


            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");
            return View("OrderCompleted", order.Id); // Trang xác nhận hoàn thành đơn hàng
        }
        public IActionResult RemoveFromCart(int productId)
        {
            // Lấy thông tin giỏ hàng từ cookie
            var cartJson = Request.Cookies["Cart"];
            if (string.IsNullOrEmpty(cartJson)) // Kiểm tra nếu cookie không tồn tại hoặc rỗng
            {
                return View("Empty");
            }

            var cart = JsonConvert.DeserializeObject<ShoppingCart>(cartJson);

            // Xóa sản phẩm có productId khỏi giỏ hàng
            cart.Items.RemoveAll(item => item.ProductId == productId);

            // Lưu lại giỏ hàng vào cookie
            Response.Cookies.Append("Cart", JsonConvert.SerializeObject(cart));

            return RedirectToAction("Index");
        }
    }
}
