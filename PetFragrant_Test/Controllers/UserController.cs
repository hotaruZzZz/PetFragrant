using Microsoft.AspNetCore.Mvc;
using PetFragrant_Test.Models;
using PetFragrant_Test.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IO;
using Microsoft.Extensions.Configuration;
using PetFragrant_Test.ViewModels;
using System;
using static Google.Apis.Requests.BatchRequest;
using ECPay.Payment.Integration;
using System.Threading.Channels;

namespace PetFragrant_Test.Controllers
{
    public class UserController : Controller
    {
        private readonly PetContext _ctx;
        private readonly IConfiguration _config;
        public UserController(PetContext ctx, IConfiguration config)
        {
            _ctx = ctx;
            _config = config;
        }

        // 使用者資訊
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if(User.Identity.IsAuthenticated)
            {
                var user = await _ctx.Customers
                  .FirstOrDefaultAsync(u => u.CustomerId == UserID());
                var userInfo = new ApplicationUser
                {
                    UserID = user.CustomerId,
                    Name = user.CustomerName,
                    Email = user.Email,
                    PhoneNo = user.PhoneNumber,
                    IsAdmin = user.IsAdmin,
                    Level = user.Level
                };
                ViewBag.User = userInfo;
                return View(userInfo);
            }
            else
            {
                return null;
            }
        }

        // 編輯使用者資料
        [Authorize]
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if(id != UserID())
            {
                return Redirect("/Account/Forbidden");
            }
            else
            {
                var user = _ctx.Customers
                    .FirstOrDefault(u => u.CustomerId == id);
                var userInfo = new ApplicationUser
                {
                    UserID = user.CustomerId,
                    Name = user.CustomerName,
                    Email = user.Email,
                    PhoneNo = user.PhoneNumber,
                    IsAdmin = user.IsAdmin
                };
                return View(userInfo);
            }

        }
        // 編輯使用者資料
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit([Bind("UserID, Name, PhoneNo")]ApplicationUser user, IFormFile file)
        {
            var customer = _ctx.Customers.First(c => c.CustomerId == user.UserID);
            if(file != null)
            {
                string uploadFolder = Path.Combine("wwwroot", "images");
                uploadFolder = Path.Combine(uploadFolder, "User");
                string folderPath = Path.Combine(uploadFolder, user.UserID);


                Directory.CreateDirectory(folderPath);

                string fileName = user.UserID + ".png";
                string filePath = Path.Combine(folderPath, fileName);

                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            if(customer != null)
            {
                customer.CustomerName = user.Name;
                customer.PhoneNumber = user.PhoneNo;
                _ctx.Update(customer);
                _ctx.SaveChanges();
                return Redirect("/User/Index/");
            }
            return RedirectToAction("/User/Index");
        }

        [Authorize]
        public IActionResult Coupon()
        {
            var all = _ctx.Discounts;
            List<CouponViewModel> coupons = new List<CouponViewModel>();
            foreach(var item in all)
            {
                if(item.Period > DateTime.Now)
                {
                    coupons.Add(new CouponViewModel { Name = item.DiscoutName, Type = item.DiscountType, Value = item.DiscountValue, Description = item.Description, Period = item.Period });
                }
            }
            return View(coupons);
        }

        //追蹤好物
        [Authorize]
        public IActionResult likes()
        {
            if (User.Identity.IsAuthenticated)
            {
                var products = _ctx.Products
            .Include(p => p.MyLikes)
                .ThenInclude(pp => pp.Customer)
            .Where(p => p.MyLikes.Any(pp => pp.CustomerId.Equals(UserID())));

                IEnumerable<ProductViewModel> productVM = products.Select(p => new ProductViewModel
                {
                    ProductData = p
                });

                return View(productVM);
            }
            return null;
        }

        // 取得使用者ID
        private string UserID()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Claims.FirstOrDefault(c => c.Type == "UserID");

                if (user != null)
                {
                    string userId = user.Value;
                    return userId;
                }
            }
            return null;
        }

        // 新增好物
        [Authorize]
        public async Task<IActionResult> CreateLike(string? id)
        {
            if (User.Identity.IsAuthenticated && id != null)
            {
                var user = await _ctx.Customers.FirstOrDefaultAsync(u => u.CustomerId == UserID());
                if (user != null)
                {
                    string userId = user.CustomerId;

                    // 檢查是否已經存在追蹤資料
                    bool alreadyLiked = _ctx.MyLikes.Any(ml => ml.ProdcutId == id && ml.CustomerId == userId);
                    if (!alreadyLiked)
                    {
                        Product product = _ctx.Products.FirstOrDefault(p => p.ProdcutId == id);
                        if (product != null)
                        {
                            MyLike myLike = new MyLike
                            {
                                ProdcutId = id,
                                CustomerId = userId
                            };
                            _ctx.Add(myLike);
                            _ctx.SaveChanges();
                            return Redirect("/Products/ProductDetail/" + id);
                        }
                    }
                    else
                    {
                        var Like = _ctx.MyLikes.FirstOrDefault(p => p.ProdcutId  == id && p.CustomerId == userId);
                        _ctx.MyLikes.Remove(Like);
                        _ctx.SaveChanges();
                        return Redirect("/Products/ProductDetail/" + id);
                    }
                }

            }

            return Redirect("/Products/ProductDetail/" + id);
        }

        // 取消追蹤
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Unfollow(string id)
        {
            if (id != null)
            {
                var like = await _ctx.MyLikes.FirstAsync(like => like.ProdcutId == id && like.CustomerId == UserID());
                if (like != null)
                {
                    _ctx.Remove(like);
                    await _ctx.SaveChangesAsync();

                }
            }
            else
            {
                return NotFound();
            }
            return RedirectToAction("User/likes");
        }

        [HttpPost]
        public IActionResult AddProduct([FromForm] string productID, [FromForm] int quantity, [FromForm] string spec)
        {
            // 檢查用戶是否已經驗證
            if (User.Identity.IsAuthenticated)
            {
                var user = _ctx.Customers.FirstOrDefault(u => u.CustomerId == UserID());
                if (user != null && productID != null && quantity != 0)
                {
                    string userId = user.CustomerId;
                    if (spec == null)
                    {
                        spec = "-1";
                    }
              
                    // 建立您的購物車物件
                    ShoppingCart shopping = new ShoppingCart
                    {
                        CustomerId = userId,
                        ProdcutId = productID,
                        SpecId = spec,
                        Quantity = quantity
                    };

                    // 將購物車物件添加到資料庫
                    _ctx.Add(shopping);
                    _ctx.SaveChanges();

                    // 返回成功結果給前端
                    return Json(new { success = true, message = "產品已成功加入購物車！" });
                }
            }

            // 如果用戶未驗證或其他錯誤情況，返回失敗結果給前端
            return Json(new { success = false, message = "發生錯誤，無法將產品加入購物車。" });
        }


        // 購物車頁面
        [Authorize]
        public IActionResult ShoppingCart()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = _ctx.Customers.Find(UserID());
                var cart = _ctx.ShoppingCarts
                    .Include(p => p.Spec)
                    .Include(p => p.Prodcut)
                        .ThenInclude(p => p.ProductSpecs)
                            .ThenInclude(p => p.Spec)
                    .Where(p => p.CustomerId.Equals(UserID()));

                IEnumerable<ShoppingCartViewModel> cartViewModels = cart.Select(c => new ShoppingCartViewModel
                {
                    ProductData = new ProductViewModel
                    {
                        ProductData = c.Prodcut,
                        SpecData = c.Prodcut.ProductSpecs.Select(ps => ps.Spec)
                    },
                    ShoppingCartData = c
                }).ToList();
                var used = _ctx.Orders.Where(c => c.CouponID != null && c.CustomerId == UserID()).Select(c => c.CouponID);
                var coupon = _ctx.Discounts
                    .Where(c => c.Period >= DateTime.Now && c.Start <= DateTime.Now &&  
                    !used.Contains(c.DiscoutID) && c.User == "所有人" || c.User == user.Level);
                List<CouponViewModel> coupons = coupon.Select(c => new CouponViewModel
                {
                    ID = c.DiscoutID,
                    Name = c.DiscoutName,
                    Description = c.Description,
                    Value = c.DiscountValue,
                    Period = c.Period,
                    Type = c.DiscountType,
                    Start = c.Start,
                    MinimumAmount = c.MinimumAmount,
                    User = c.User

                }).ToList();
                ViewData["coupon"] = coupons;
                return View(cartViewModels);
            }
            return null;
        }

        // 從購物車移除商品
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveProduct(string id)
        {
            string userId = UserID();
            if(id != null)
            {
                var cart = await _ctx.ShoppingCarts.FirstAsync(p => p.ProdcutId == id && p.CustomerId == userId);
                _ctx.Remove(cart);
                await _ctx.SaveChangesAsync();
            }
            else
            {
                return NotFound();
            }
            return RedirectToAction("ShoppingCart");
        }

        [Authorize]
        // 計算購物車有多少商品
        public async Task<IActionResult> CountProduct()
        {
            string userId = UserID();
            int total = await _ctx.ShoppingCarts.Where(p => p.CustomerId == userId).CountAsync();
            ViewBag.TotalCount = total;
            return PartialView("_NavbarPartial", total);

        }


        // 測試用
        public IActionResult TestPost()
        {
            return View();
        }
        [HttpPost]
        public ContentResult TestPost(IFormCollection collection)
        {
            return null;
        }

        // 測試用


        public IActionResult Test()
        {


            return View();
        }

        // 忘記密碼
        public IActionResult ForgotPsw(string id)
        {
            return View();
        }
        [Authorize]
        // 問題回報
        public IActionResult Report()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Report(string Title, string Description)
        {
            if(Title != null)
            {
                Report report = new Report()
                {
                    CustomerId = UserID(),
                    Title = Title,
                    Description = Description
                };
                _ctx.Add(report);
                _ctx.SaveChanges();
            }
            return Redirect("/User/");
        }

        [HttpPost]
        [Authorize]
        public Task<JsonResult> UpdateCartSpec(string oldspecID, string specID, string productID)
        {
            using (var transaction = _ctx.Database.BeginTransaction())
            {
                try
                {
                    ShoppingCart cart = _ctx.ShoppingCarts.FirstOrDefault(p => p.SpecId == oldspecID && p.ProdcutId == productID &&
                                p.CustomerId == UserID());
                    cart.SpecId = specID;
                    _ctx.Update(cart);
                    _ctx.SaveChanges();

                    return Task.FromResult(Json(new { success = true }));
                }
                catch(Exception ex)
                {
                    // 發生錯誤時回滾交易
                    transaction.Rollback();
                    // 可以處理或記錄錯誤訊息

                    throw;
                }
            }

        }

        // 取消訂單 
        [Authorize]
        public IActionResult CancelOrder(string id)
        {
            ViewData["id"] = id;

            return View();
        }
        [HttpPost]
        public IActionResult CancelOrder(string id, string CancelReason)
        {
            if(id != null)
            {
                Order order = _ctx.Orders.Find(id);
                if(order != null)
                {
                    order.status = "顧客取消，原因:" + CancelReason;
                    order.Check = true;
                    _ctx.Update(order);
                    _ctx.SaveChanges();
                    
                }
            }
            return Redirect("/Orderlist/MyOrder");
        }

    }
}
