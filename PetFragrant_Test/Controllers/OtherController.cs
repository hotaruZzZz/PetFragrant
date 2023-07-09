using Microsoft.AspNetCore.Mvc;

namespace PetFragrant_Test.Controllers
{
    public class OtherController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Problem()
        {
            return View();
        }
    }
}
