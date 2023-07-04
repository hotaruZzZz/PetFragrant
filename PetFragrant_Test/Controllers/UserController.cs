using Microsoft.AspNetCore.Mvc;
using PetFragrant_Test.Models;
using PetFragrant_Test.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CoreMvc5_CookieAuthentication.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace PetFragrant_Test.Controllers
{
    public class UserController : Controller
    {
        private readonly PetContext _ctx;
        public UserController(PetContext ctx)
        {
            _ctx = ctx;
        }

        // 使用者資訊
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if(User.Identity.IsAuthenticated)
            {
                var user = await _ctx.Customers
                  .FirstOrDefaultAsync(u => u.CustomerName == User.Identity.Name);
                var userInfo = new ApplicationUser
                {
                    Name = user.CustomerName,
                    Email = user.Email,
                    PhoneNo = user.PhoneNumber,
                    IsAdmin = user.IsAdmin
                };
                ViewBag.User = userInfo;
                return View(userInfo);
            }
            else
            {
                return null;
            }
        }

        //追蹤好物
            [Authorize]
            public async Task<IActionResult> likes()
            {
                if (User.Identity.IsAuthenticated)
                {

                var user = await _ctx.Customers
                  .FirstOrDefaultAsync(u => u.CustomerName == User.Identity.Name);
                string userId = user.CustomerId;
                var products = _ctx.Products
            .Include(p => p.MyLikes)
                .ThenInclude(pp => pp.Customer)
            .Where(p => p.MyLikes.Any(pp => pp.CustomerId.Equals(userId)));
                
                return View(products);
                }
                return null;
            }

        private async Task<string> UserID()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _ctx.Customers.FirstOrDefaultAsync(u => u.CustomerName == User.Identity.Name);
                if (user != null)
                {
                    string userId = user.CustomerId;
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
                var user = await _ctx.Customers.FirstOrDefaultAsync(u => u.CustomerName == User.Identity.Name);
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
                var user = await _ctx.Customers.FirstOrDefaultAsync(u => u.CustomerName == User.Identity.Name);
                if (user != null)
                {
                    string userId = user.CustomerId;

                    ShoppingCart shopping = new ShoppingCart{
                        CustomerId = userId,
                        ProdcutId=productID,
                        SpecId = spec,
                        Quantity = quantity
                    };
                    _ctx.Add(shopping);
                    _ctx.SaveChanges();
                }

            }
            return null;
        }

        // 購物車頁面
        [Authorize]
        public async Task<IActionResult> ShoppingCart()
        {
            if (User.Identity.IsAuthenticated)
            {

                var user = await _ctx.Customers
                  .FirstOrDefaultAsync(u => u.CustomerName == User.Identity.Name);
                string userId = user.CustomerId;
                var products = _ctx.Products
                    .Include(p => p.ShoppingCarts)
                    .Include(p => p.ProductSpecs)
                        .ThenInclude(ps => ps.Spec)
                    .Where(p => p.ShoppingCarts.Any(pp => pp.CustomerId.Equals(userId)));
                
                return View(products);
            }
            return null;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveProduct(string id)
        {
            string userId = await UserID();
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
            string userId = await UserID();
            int total = await _ctx.ShoppingCarts.Where(p => p.CustomerId == userId).CountAsync();
            ViewBag.TotalCount = total;
            return PartialView("_NavbarPartial", total);

        }
    }
}
