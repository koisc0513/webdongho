using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WatchStore.Models;
using WatchStore.Repository;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace WatchStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        
        private IProductRepository _productRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        private IUserRepository _userRepository;

        public AdminController(
            IProductRepository productRepository,
            IHttpContextAccessor httpContextAccessor,
            
            IUserRepository userRepository)
        {
           
            _productRepository = productRepository;
            _httpContextAccessor = httpContextAccessor;
          
            _userRepository = userRepository;
        }
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
        private void SetAuthenticationCookie(int userId)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
    };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
                ).Wait();
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
            User existingUser = _userRepository.GetUserByEmail(user.Email);

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


        public IActionResult Index()
        {
            return View();
        }
        
       
        
      
    }

}

