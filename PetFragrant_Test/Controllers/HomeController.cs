using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetFragrant_Test.Data;
using PetFragrant_Test.Models;
using PetFragrant_Test.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PetFragrant_Test.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PetContext _petContext;
        public HomeController(ILogger<HomeController> logger, PetContext petContext)
        {
            _petContext = petContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await　_petContext.Products.ToListAsync());
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult JoinMe()
        {
            return View();
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
