using Microsoft.AspNetCore.Mvc;

namespace PetFragrant_Test.Controllers
{
    public class BackstageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
