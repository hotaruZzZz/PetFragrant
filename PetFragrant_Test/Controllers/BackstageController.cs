using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetFragrant_Test.Data;
using PetFragrant_Test.Models;
using PetFragrant_Test.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetFragrant_Test.Controllers
{
    public class BackstageController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PetContext _ctx;
        public BackstageController(ILogger<HomeController> logger, PetContext ctx)
        {
            _ctx = ctx;
            _logger = logger;
        }

        [Authorize(Policy = "IsAdmin")]
        public IActionResult Index()
        {
            return View();
        }

        // 管理折價券
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public IActionResult ManageCoupon()
        {
            var coupon = _ctx.Discounts.ToList();
            List<CouponViewModel> coupons = coupon.Select(c => new CouponViewModel
            {
                ID = c.DiscoutID,
                Name = c.DiscoutName,
                Description = c.Description,
                Value = c.DiscountValue,
                Period = c.Period,
                Type = c.DiscountType

            }).ToList();
            return View(coupons);
        }

        // 新增折價券
        [Authorize]
        public IActionResult CreateCoupon()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoupon([Bind("DiscoutName, Description, DiscountValue, DiscountType, Period")]Discount discount)
        {
            if (ModelState.IsValid)
            {
                _ctx.Add(discount);
                await _ctx.SaveChangesAsync();
                return RedirectToAction(nameof(ManageCoupon));
            }
            return View(discount);
        }
    }
}
