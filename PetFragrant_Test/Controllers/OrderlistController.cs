using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PetFragrant_Test.Controllers
{
    public class OrderlistController : Controller
    {
        [Authorize]
        public IActionResult MyOrder()
        {
            return View();
        }
        [Authorize]
        public IActionResult History()
        {
            return View();
        }
    }
}
