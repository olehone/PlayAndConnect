using Microsoft.AspNetCore.Mvc;
using PlayAndConnect.Models;
using System.Diagnostics;
using PlayAndConnect.Data;
using PlayAndConnect.Data.Interfaces;

namespace PlayAndConnect.Controllers
{
    public class HomeController : Controller
    {
        //_httpContext.Request.Cookies["username"]
        private readonly ApplicationDbContext _db;
        private readonly HttpContext _httpContext;
        private readonly UserDb _userDb;
        public HomeController(ApplicationDbContext db, IHttpContextAccessor httpContext)
        {
            _db = db;
            _httpContext = httpContext.HttpContext;
            _userDb = new UserDb(_db);
        }  
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            //add username in cookies and redirect if allgood
            if (username ==""|| password =="1")//await _userDb.Verify(username, password))
            {
                _httpContext.Response.Cookies.Append("username", username);
                return new RedirectResult("Index");
            }
            else
                return new RedirectResult("Login");
        }
        public IActionResult Singup()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Singup(User user)
        {
            await _userDb.Create(user);
            _httpContext.Response.Cookies.Append("username", "value");
            return View("Login");
        }

        public IActionResult Index()
        {
            if(_httpContext.Request.Cookies["username"]!=null)
            {
                ViewBag.username = _httpContext.Request.Cookies["username"];
                return View("Index");
            }   
            else
                return View("IndexForNew");
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