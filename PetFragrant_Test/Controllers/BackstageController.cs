using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetFragrant_Test.Data;
using PetFragrant_Test.Models;
using PetFragrant_Test.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

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
            var orders = _ctx.Orders
                .Include(o => o.OrderDetails).
                ThenInclude(od => od.Prodcut);
            ViewData["today"] = _ctx.Orders.Where(p => p.Orderdate.Month == DateTime.Now.Month).Count();
            ViewData["pending"] = orders.Where(o => o.Check == false).Count();
            decimal total = 0;
            foreach(var o in orders)
            {
                Discount discount = new Discount();

                if(o.Orderdate.Month == DateTime.Now.Month)
                {
                    if (o.CouponID != null)
                    {
                        discount = _ctx.Discounts.Find(o.CouponID);
                        switch (discount.DiscountType)
                        {
                            case "折抵":
                                total += o.OrderDetails.Sum(od => Math.Round(od.Prodcut.Price * (decimal)0.9) * od.Amount) - discount.DiscountValue;
                                break;
                            case "折扣(%)":
                                total += o.OrderDetails.Sum(od => Math.Round(od.Prodcut.Price * (decimal)0.9) * od.Amount) * discount.DiscountValue;
                                break;

                        }
                    }
                    else
                    {
                        total += o.OrderDetails.Sum(od => Math.Round(od.Prodcut.Price * (decimal)0.9) * od.Amount);
                    }
                }
                
            }
            ViewData["total"] = total;
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
                Start = c.Start,
                Period = c.Period,
                Type = c.DiscountType,
                MinimumAmount = c.MinimumAmount,
                User = c.User
            }).ToList();
            return View(coupons);
        }

        // 新增折價券
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public IActionResult CreateCoupon()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoupon(Discount discount)
        {
            if (ModelState.IsValid)
            {
                _ctx.Add(discount);
                await _ctx.SaveChangesAsync();
                return RedirectToAction(nameof(ManageCoupon));
            }
            return View(discount);
        }

        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> EditCoupon(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _ctx.Discounts.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }
            CouponViewModel couponVM = new CouponViewModel()
            {
                ID = coupon.DiscoutID,
                Name = coupon.DiscoutName,
                Description = coupon.Description,
                Value = coupon.DiscountValue,
                Start = coupon.Start,
                Period = coupon.Period,
                MinimumAmount = coupon.MinimumAmount,
                User = coupon.User,
                Type = coupon.DiscountType
            };
            return View(couponVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCoupon(string id, CouponViewModel couponVM)
        {
            if (id != couponVM.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Discount discount = await _ctx.Discounts.FindAsync(id);
                discount.DiscoutName = couponVM.Name;
                discount.DiscountValue = couponVM.Value;
                discount.Start = couponVM.Start;
                discount.Period = couponVM.Period;
                discount.DiscountType = couponVM.Type;
                discount.User = couponVM.User;
                discount.MinimumAmount = couponVM.MinimumAmount;
                discount.Description = couponVM.Description;
                _ctx.Update(discount);
                await _ctx.SaveChangesAsync();
            }
    
            return RedirectToAction(nameof(ManageCoupon));
        }

        [HttpPost]

        public async Task<IActionResult> DeleteCouponNow(string id)
        {
            Discount discount = _ctx.Discounts.Find(id);
            _ctx.Discounts.Remove(discount);
            await _ctx.SaveChangesAsync();
            return Redirect("/Backstage/ManageCoupon");
        }
        [Authorize(Policy = "IsAdmin")]
        public IActionResult ManageUser()
        {
            var users = _ctx.Customers.ToList();
            List<ApplicationUser> usersList = new List<ApplicationUser>();
            foreach (var user in users)
            {
                usersList.Add(new ApplicationUser
                {
                    UserID = user.CustomerId,
                    Name = user.CustomerName,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin,
                    Level = user.Level
                });
            }
            return View(usersList);
        }
        [Authorize(Policy = "IsAdmin")]
        public IActionResult EditUser(string id)
        {
            ApplicationUser user = ApplicationUser(id);
            if (user != null)
            {
                return View(user);
            }
            return View();
        }
        [HttpPost]
        public IActionResult EditUser(ApplicationUser user)
        {
            if(user != null)
            {
                var customer = _ctx.Customers.Find(user.UserID);
                if(customer != null)
                {
                    customer.CustomerName = user.Name;
                    customer.Email = user.Email;
                    customer.IsAdmin = user.IsAdmin;
                    customer.Level = user.Level;
                    _ctx.Update(customer);
                    _ctx.SaveChanges();
                }

            }
            return Redirect("/Backstage/ManageUser");
        }
        [Authorize(Policy = "IsAdmin")]
        private ApplicationUser ApplicationUser(string id)
        {
            var user = _ctx.Customers.Find(id);
            if (user != null) {
                ApplicationUser u = new ApplicationUser
                {
                    UserID = id,
                    Name = user.CustomerName,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin,
                    Level = user.Level,
                    PhoneNo = user.PhoneNumber
                };
                return u;
            }
            else
            {
                return null;
            }
        }
        [Authorize(Policy = "IsAdmin")]
        public IActionResult ManageOrder()
        {
            var orders = _ctx.Orders.
                Include(o => o.OrderDetails)
                .ThenInclude(od => od.Prodcut).ThenInclude(p => p.ProductSpecs).ThenInclude(ps => ps.Spec)
                .OrderByDescending(o => o.Orderdate);
            List<OrderDetailViewModel> uncheck = new List<OrderDetailViewModel>();  // 未確認
            List<OrderDetailViewModel> check = new List<OrderDetailViewModel>();  // 已確認，待出貨
            List<OrderDetailViewModel> ship = new List<OrderDetailViewModel>();  // 已出貨
            List<OrderDetailViewModel> arrive = new List<OrderDetailViewModel>();  // 已到貨
            List<OrderDetailViewModel> today = new List<OrderDetailViewModel>();  //所有

            foreach (var o in orders)
            {
                OrderDetailViewModel viewModel = new OrderDetailViewModel
                {
                    OrderId = o.OrderId,
                    Check = o.Check,
                    Orderdate = o.Orderdate,
                    Shipdate = o.Shipdate,
                    PaymentDate = o.Paymentdate,
                    Payment = o.Payment,
                    Status = o.status,
                    TotalPrice = o.OrderDetails.Sum(od => Math.Round(od.Prodcut.Price * (decimal)0.9) * od.Amount)
                };
                
                if(o.Orderdate.Day == DateTime.Now.Day)
                {
                    today.Add(viewModel);
                }
                if(o.status == "未確認")
                {
                    uncheck.Add(viewModel);
                }
                else if(o.status == "已確認，處理中")
                {
                    check.Add(viewModel);
                }
                else if(o.status == "已出貨")
                {
                    ship.Add(viewModel);
                }
                else if(o.status == "已到貨")
                {
                    ship.Add(viewModel);
                }
                
            }
            ViewData["today"] = today;
            ViewData["check"] = check;
            ViewData["uncheck"] =uncheck;
            ViewData["ship"] = ship;
            ViewData["arrive"] = arrive;
            return View();
        }

        // 訂單預覽
        [Authorize(Policy = "IsAdmin")]
        public IActionResult ViewOrder(string id)
        {
            if(id != null)
            {
                var orders = _ctx.Orders.
                   Include(o => o.OrderDetails)
                   .ThenInclude(od => od.Prodcut).ThenInclude(p => p.ProductSpecs).ThenInclude(ps => ps.Spec)
                   .OrderByDescending(o => o.Orderdate).First(o => o.OrderId == id);
                if (orders != null)
                {
                    OrderDetailViewModel viewModel = new OrderDetailViewModel
                    {
                        OrderId = orders.OrderId,
                        Orderdate = orders.Orderdate,
                        Shipdate = orders.Shipdate,
                        Arriiveddate = orders.Arriiveddate,
                        PaymentDate = orders.Paymentdate,
                        Check = orders.Check,
                        Status = orders.status,
                        Payment = orders.Payment,
                        Delivery = orders.Delivery,
                        TotalPrice = orders.OrderDetails.Sum(od => Math.Round(od.Prodcut.Price * (decimal)0.9) * od.Amount),
                        StoreName = _ctx.ReciveStores.Find(orders.StoreID)?.Name,
                        orderproduct = orders.OrderDetails.Select(od => new OrderProductViewModel
                        {
                            ProdcutId = od.ProdcutId,
                            ProductName = od.Prodcut.ProductName,
                            SpecName = od.Prodcut.ProductSpecs.Select(ps => ps.Spec.SpecName).FirstOrDefault(),
                            Price = Math.Round(od.Prodcut.Price * (decimal)0.9),
                            PriceTotal = Math.Round(od.Prodcut.Price * (decimal)0.9) * od.Amount,
                            Amount = od.Amount
                        })
                    };
                    return View(viewModel);

                }
            }

            return View();
        }

        // 取消
        [HttpPost]
        [Authorize(Policy = "IsAdmin")]
        public IActionResult Cancel(string id)
        {
            if(id != null)
            {
                var orders = _ctx.Orders.First(o => o.OrderId == id);
                if(orders != null)
                {
                    orders.status = "已被取消";
                    orders.Check = true;
                    _ctx.Update(orders);
                    _ctx.SaveChanges();
                }
            }
            return Redirect("/Backstage/ManageOrder");
        }

        // 確定訂單 Confirm
        [HttpPost]
        [Authorize(Policy = "IsAdmin")]
        public IActionResult Confirm(string id)
        {
            if (id != null)
            {
                var orders = _ctx.Orders.First(o => o.OrderId == id);
                if (orders != null)
                {
                    orders.status = "已確認，處理中";
                    orders.Check = true;
                    _ctx.Update(orders);
                    _ctx.SaveChanges();
                }
            }
            return Redirect("/Backstage/ManageOrder");
        }

        // 已出貨
        [HttpPost]
        [Authorize(Policy = "IsAdmin")]
        public IActionResult Ship(string id)
        {
            if (id != null)
            {
                var orders = _ctx.Orders.First(o => o.OrderId == id);
                if (orders != null)
                {
                    orders.status = "已出貨";
                    orders.Shipdate = DateTime.Now;
                    _ctx.Update(orders);
                    _ctx.SaveChanges();
                }
            }
            return Redirect("/Backstage/ManageOrder");
        }
        // 已到貨
        [HttpPost]
        [Authorize(Policy = "IsAdmin")]
        public IActionResult Arrive(string id)
        {
            if (id != null)
            {
                var orders = _ctx.Orders.First(o => o.OrderId == id);
                if (orders != null)
                {
                    orders.status = "已到貨";
                    orders.Arriiveddate = DateTime.Now;
                    _ctx.Update(orders);
                    _ctx.SaveChanges();
                }
            }
            return Redirect("/Backstage/ManageOrder");
        }
        // 完成訂單
        [HttpPost]
        [Authorize(Policy = "IsAdmin")]
        public IActionResult Finish(string id)
        {
            if (id != null)
            {
                var orders = _ctx.Orders.First(o => o.OrderId == id);
                if (orders != null)
                {
                    orders.status = "完成";
                    _ctx.Update(orders);
                    _ctx.SaveChanges();
                }
            }
            return Redirect("/Backstage/ManageOrder");
        }
 
        //管理店家
        [Authorize(Policy = "IsAdmin")]
        public IActionResult ManageStore()
        {
            var stores = _ctx.ReciveStores;
            List<StoreViewModel> storeViews = new List<StoreViewModel>();
            foreach(var store in stores)
            {
                storeViews.Add(new StoreViewModel()
                {
                    Id = store.Id,
                    Name  = store.Name,
                    Address = store.Address
                });
            }
            return View(storeViews);
        }
        // 新增店家
        [Authorize(Policy = "IsAdmin")]
        public IActionResult CreateStore()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateStore(StoreViewModel storeView)
        {
            if (ModelState.IsValid)
            {
                if(storeView != null)
                {
                    ReciveStore store = new ReciveStore();
                    store.Id = storeView.Id;
                    store.Name = storeView.Name;
                    store.Address = storeView.Address;
                    _ctx.Add(store);
                    _ctx.SaveChanges();
                }
            }
            return Redirect("/Backstage/ManageStore");
        }

        [HttpGet]
        public IActionResult GetAddress(string id)
        {
            if (id != null)
            {
                var store = _ctx.ReciveStores.Find(id);
                if (store != null)
                {
                    var address = store.Address;
                    if (address != null)
                    {
                        return Ok(new { address = address });
                    }
                }
            }

            return BadRequest(new { message = "找不到該店家的地址。" });
        }


        public int OrderAmount(string name, string date)
        {
            if (date == null)
            {
                date = DateTime.Now.ToString("yyyy-MM");
            }

            DateTime selectedDate = DateTime.ParseExact(date, "yyyy-MM", null);


            var orders = _ctx.Orders.Where(o => o.status.Contains(name) && o.Orderdate.Month == selectedDate.Month
            && o.Orderdate.Year == selectedDate.Year);
            return orders.Count();
        }

        public int OrderAmountYear(string name, string year)
        {
            if (year == null)
            {
                year = DateTime.Now.ToString("yyyy");
            }

            DateTime selectedDate = DateTime.ParseExact(year, "yyyy", null);


            var orders = _ctx.Orders.Where(o => o.status.Contains(name) && o.Orderdate.Year == selectedDate.Year);
            return orders.Count();
        }

        public int CategoryAmount(string  name, string date)
        {
            if (date == null)
            {
                date = DateTime.Now.ToString("yyyy-MM");
            }

            DateTime selectedDate = DateTime.ParseExact(date, "yyyy-MM", null);

            var orders = _ctx.Orders
        .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Prodcut)
        .ThenInclude(p => p.Categories)
        .Where(o => o.OrderDetails.Any(od => od.Prodcut.Categories.FatherCategory.CategoryName == name) && o.Orderdate.Month == selectedDate.Month); 
            return orders.Count();
        }

        public int CategoryAmountYear(string name, string year)
        {
            if (year == null)
            {
                year = DateTime.Now.ToString("yyyy");
            }

            DateTime selectedDate = DateTime.ParseExact(year, "yyyy", null);

            var orders = _ctx.Orders
        .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Prodcut)
        .ThenInclude(p => p.Categories)
        .Where(o => o.OrderDetails.Any(od => od.Prodcut.Categories.FatherCategory.CategoryName == name) && o.Orderdate.Year == selectedDate.Year);
            return orders.Count();
        }

        [Authorize(Policy = "IsAdmin")]
        public IActionResult ManageReport()
        {
            var reports = _ctx.Reports.Include(r => r.Customer);
            List<ReportViewModel> reportVM = new List<ReportViewModel>();
            foreach (var report in reports)
            {
                reportVM.Add(new ReportViewModel()
                {
                    Email = report.Customer.Email,
                    Title = report.Title,
                    Description = report.Description,
                });
            }
            return View(reportVM);
        }
    }
}
