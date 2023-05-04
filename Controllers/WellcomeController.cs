using Microsoft.AspNetCore.Mvc;
using PlayAndConnect.Models;
using System.Diagnostics;
using PlayAndConnect.Data;
using PlayAndConnect.Data.Interfaces;

namespace PlayAndConnect.Controllers
{
    public class WellcomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly HttpContext _httpContext;
        private readonly UserDb _userDb;
        public WellcomeController(ApplicationDbContext db, IHttpContextAccessor httpContext)
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
            if (await _userDb.Verify(username, password))
            {
                _httpContext.Response.Cookies.Append("username", username);
                return new RedirectResult("Home/Index");
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
            _httpContext.Response.Cookies.Append("login", "value");
            return View("Login");
        }
    }
}