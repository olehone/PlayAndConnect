using Microsoft.AspNetCore.Mvc;

namespace PlayAndConnect.Controllers
{
    public class HomeController1 : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}
