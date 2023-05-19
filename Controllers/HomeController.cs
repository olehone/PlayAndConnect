using System.Security.AccessControl;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PlayAndConnect.Models;
using System.Diagnostics;
using PlayAndConnect.Data;
using PlayAndConnect.Data.Interfaces;
using PlayAndConnect.ViewModels;
using System.Security.Claims;
using Pomelo.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace PlayAndConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserDb _userDb;
        public HomeController(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, IUserDb userDb)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _userDb = userDb;
        }
        [HttpGet]
        [Authorize]
        public IActionResult AddGame()
        {
            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddGame(string title, string description)
        {
            return View("Index");
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            await Logout();
            if (ModelState.IsValid)
            {
                User? user = await _db.Users.FirstOrDefaultAsync<User>(u => u.Login == username && u.PasswordHash == Hashing.HashPassword(password));
                if (user != null)
                {
                    await Authenticate(user.Login);
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Логін або пароль неправильні";
                    return RedirectToAction("Login");
                }
            }
            else
                return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Signup()
        {
            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
            }
            if (TempData.ContainsKey("username"))
            {
                ViewBag.username = TempData["username"];
                TempData.Remove("username");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(string username, string password, string confirm_password)
        {
            await Logout();
            if (ModelState.IsValid)
            {
                if (password != confirm_password)
                {
                    TempData["username"] = username;
                    TempData["Error"] = "Паролі не співпадають";
                    return RedirectToAction("Signup");
                }
                User? user = _db.Users.FirstOrDefault<User>(u => u.Login == username);
                if (user == null)
                {
                    UserInfo newUserInfo = new UserInfo { Name = username };
                    User newUser = new User { Login = username, PasswordHash = Hashing.HashPassword(password) };
                    newUser.Info = newUserInfo;
                    newUserInfo.User = newUser;
                    /*ICollection<Game>? userGames = new List<Game> ();
                    userGames.Add()
                    newUser.Games = userGames;*/
                    _db.Infos.Add(newUserInfo);
                    _db.Users.Add(newUser);
                    _db.SaveChanges();
                    await Authenticate(newUser.Login);
                    return RedirectToAction("Index");

                }
                else
                {
                    TempData["Error"] = "Користувач існує";
                    return RedirectToAction("Signup");
                }
            }
            else
            {
                ViewBag.Error = "erorus";
                return View();
            }
        }
        public IActionResult Index()
        {
            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
            }
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null
                && _httpContextAccessor.HttpContext.User != null &&
                _httpContextAccessor.HttpContext.User.Identity != null &&
                !string.IsNullOrEmpty(_httpContextAccessor.HttpContext.User.Identity.Name))
            {
                ViewBag.username = _httpContextAccessor.HttpContext.User.Identity.Name;
                return View();
            }
            else
            {
                return View("IndexForNew");
            }

        }
        [HttpGet]
        [Authorize]
        public IActionResult Settings()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Settings(int age = 342347653, string name = "t#45f/**sdf")
        {
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null
                && _httpContextAccessor.HttpContext.User != null &&
                _httpContextAccessor.HttpContext.User.Identity != null &&
                !string.IsNullOrEmpty(_httpContextAccessor.HttpContext.User.Identity.Name))
            {
                string? username = _httpContextAccessor.HttpContext.User.Identity.Name;
                UserInfo? userInfo = await _db.Infos.FirstOrDefaultAsync<UserInfo>(i => i.User.Login == username);
                if (userInfo != null)
                {
                    if (age != 342347653)
                        userInfo.Age = age;
                    if (name != "t#45f/**sdf")
                        userInfo.Name = name;
                    _db.Infos.Update(userInfo);
                    await _db.SaveChangesAsync();
                    return View();
                }
                else
                    return RedirectToAction("Index");
            }
            else
            {
                return View("IndexForNew");
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}