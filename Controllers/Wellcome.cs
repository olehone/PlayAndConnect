using Microsoft.AspNetCore.Mvc;
using PlayAndConnect.Data;

namespace PlayAndConnect.Controllers
{
    public class WellcomeController : Controller
    {
        ApplicationDbContext _db;
        public WellcomeController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
