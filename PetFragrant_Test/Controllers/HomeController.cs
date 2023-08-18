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
        private readonly PetContext _ctx;
        public HomeController(ILogger<HomeController> logger, PetContext ctx)
        {
            _ctx = ctx;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // 分類
            ViewData["Food"] = _ctx.Categories.Where(c => c.FatherCategory.CategoryName == "食品");
            ViewData["Out"] = _ctx.Categories.Where(c => c.FatherCategory.CategoryName == "外出");
            ViewData["Toy"] = _ctx.Categories.Where(c => c.FatherCategory.CategoryName == "玩具");
            ViewData["Home"] = _ctx.Categories.Where(c => c.FatherCategory.CategoryName == "居家");
            ViewData["Health"] = _ctx.Categories.Where(c => c.FatherCategory.CategoryName == "保健");
            ViewData["Beauty"] = _ctx.Categories.Where(c => c.FatherCategory.CategoryName == "美容");
            ViewData["recom"] = _ctx.Products.OrderByDescending(p => p.Inventory).Take(8).ToList();
            // 貓&狗商品
            ViewData["dog"] = _ctx.Products.Where(c => c.ProductName.Contains("狗")).Take(4);
            ViewData["cat"] = _ctx.Products.Where(c => c.ProductName.Contains("貓")).Take(4);
            // 關鍵字
            ViewData["keyword"] = _ctx.Keywords.OrderBy(k => k.Amount).Take(8).ToList();
            return View(await　_ctx.Products.ToListAsync());
        }

        public JsonResult SearchKeyword(string keyword)
        {
            var product = _ctx.Products.Where(p => p.ProductName.Contains(keyword)).Select(p => p.ProductName).Take(6).ToList();
            return Json(product);
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult JoinMe()
        {
            return View();
        }

        public IActionResult CustomerLevel()
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
