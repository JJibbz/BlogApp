using BlogApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using NLog;

namespace BlogApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);

                if (user != null && user.Role != null)
                {
                   
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email ?? string.Empty), // Проверка на NULL
                new Claim(ClaimTypes.Role, user.Role.Name ?? string.Empty),// Проверка на NULL
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // Проверка на NULL

            };
               

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    
                    Logger.Info($"Попытка входа пользователя с email: {user.Email}");
                    // Redirect to the desired page after login
                    return RedirectToAction("Index", "Home");
                    
                }

                // User not found or invalid password
                ModelState.AddModelError("", "Invalid login attempt.");
            }
            else
            {
                Logger.Info($"Ошибка входа пользователя с email: {model.Email}");
                // Log the invalid model state for debugging
                Console.WriteLine("Model state is invalid.");
            }

            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            Logger.Info($"Выход пользователя");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Email = model.Email,
                    Password = model.Password,
                    RoleId = 3
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                Logger.Info($"Успешная регистрация пользователя ${model.Email}");

                // Перенаправление на страницу входа после успешной регистрации
                return RedirectToAction("Login", "Account");
            }

            Logger.Info($"Неудачная регистрация пользователя ${model.Email}");


            return View(model);
        }

    }
}