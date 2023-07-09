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
        //追蹤好物
        [Authorize]
            public async Task<IActionResult> likes()
            {
                if (User.Identity.IsAuthenticated)
                {

                var user = await _ctx.Customers
                  .FirstOrDefaultAsync(u => u.CustomerId == UserID());
                string userId = user.CustomerId;
                var products = _ctx.Products
            .Include(p => p.MyLikes)
                .ThenInclude(pp => pp.Customer)
            .Where(p => p.MyLikes.Any(pp => pp.CustomerId.Equals(userId)));
                
                return View(products);
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


        // 加入購物車 
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddProduct(string productID, int quantity, string spec)
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = await _ctx.Customers.FirstOrDefaultAsync(u => u.CustomerId == UserID());
                if (user != null)
                {
                    string userId = user.CustomerId;
                    if(spec == null)
                    {
                        spec = "-1";
                    }
                    ShoppingCart shopping = new ShoppingCart{
                        CustomerId = UserID(),
                        ProdcutId=productID,
                        SpecId = spec,
                        Quantity = quantity
                    };
                    var s = shopping;
                    _ctx.Add(shopping);
                    _ctx.SaveChanges();
                }

            }
            return null;
        }

        // 購物車頁面
        [Authorize]
        public IActionResult ShoppingCart()
        {
            if (User.Identity.IsAuthenticated)
            {

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


                return View(cartViewModels);
            }
            return null;
        }

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

        public async Task<IActionResult> CountProduct()
        {
            string userId = UserID();
            int total = await _ctx.ShoppingCarts.Where(p => p.CustomerId == userId).CountAsync();
            ViewBag.TotalCount = total;
            return PartialView("_NavbarPartial", total);

        }

        [HttpPost]
        public IActionResult Unfollow(string id)
        {
            var like = _ctx.MyLikes.First(like => like.ProdcutId == id);
            if(like != null)
            {
                _ctx.Remove(like);
                _ctx.SaveChanges();
                return RedirectToAction("User/likes");
            }
            return View();
        }

        [Authorize]
        public IActionResult Test()
        {
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



            return View(cartViewModels);
        }

        public IActionResult ForgotPsw(string id)
        {
            return View();
        }
    }
}
