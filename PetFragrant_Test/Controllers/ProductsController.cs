using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetFragrant_Test.Data;
using PetFragrant_Test.Models;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using PetFragrant_Test.ViewModels;

namespace PetFragrant_Test.Controllers
{
    public class ProductsController : Controller
    {
        private readonly PetContext _context;

        public ProductsController(PetContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var petContext = _context.Products.Include(p => p.Categories);
            return View(await petContext.ToListAsync());
        }

        public async Task<IActionResult> ProductList()
        {
            return View(await _context.Products.ToListAsync());
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

        public IActionResult ProductDetail(string? id)
        {
            if(id != null)
            {
                var data = _context.Products
                    .Include(p => p.Categories)
                        .ThenInclude(p => p.FatherCategory)
                    .Include(p => p.ProductSpecs)
                        .ThenInclude(ps => ps.Spec)
                    .Where(p => p.ProdcutId == id).FirstOrDefault();

                ProductViewModel products = new ProductViewModel
                {
                    ProductData = data,
                    CategoryData = data.Categories,
                    SpecData = data.ProductSpecs.Select(ps => ps.Spec)
                };

                DirectoryInfo piclen = new DirectoryInfo(@"wwwroot\images\" + products.ProductData.ProdcutId);
                var user = _context.Customers.FirstOrDefault(u => u.CustomerId == UserID());
                bool trace = false;
                int len = piclen.GetFiles("*.png").Length;
                // 當使用者有登入，確認是否有追蹤此商品
                if (user != null)
                {
                    trace = _context.MyLikes.Any(p => p.ProdcutId == id && p.CustomerId == user.CustomerId);
                }

                ViewData["IsLike"] = trace;
                ViewData["len"] = len;


                return View(products);
            }
            else
            {
                return NotFound();
            }
        }


        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoriesID"] = new SelectList(_context.Categories.Where(c => c.FatherCategoryId == null), "CategoryId", "CategoryName");
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ProdcutId, CategoriesId,ProductName,ProductDescription,Price,Inventory")] Product product)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var data = _context.Products.FirstOrDefault(p => p.ProdcutId == product.ProdcutId);
                    if (data != null)
                    {
                        data.ProductName = product.ProductName;
                        data.ProductDescription = product.ProductDescription;
                        data.CategoriesId = product.CategoriesId;
                        data.Price = product.Price;
                        data.Inventory = product.Inventory;
                        _context.Update(data);
                        _context.SaveChanges();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProdcutId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriesID"] = new SelectList(_context.Categories, "CategoryID", "CategoryID", product.CategoriesId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(m => m.ProdcutId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            string folderPath = Path.Combine("wwwroot", "images");
            folderPath = Path.Combine(folderPath, id);
            //var fileSystem = new FileSystem();
            FileSystem.DeleteDirectory(folderPath, UIOption.AllDialogs, RecycleOption.DeletePermanently);
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "IsAdmin")]
        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewData["CategoriesID"] = new SelectList(_context.Categories.Where(c => c.FatherCategoryId == null), "CategoryId", "CategoryName");
            return View();
        }
        public JsonResult GetSubcategory(string id)
        {
            var category = _context.Categories.Where(c => c.FatherCategoryId.Equals(id));
            return Json(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct([Bind("CategoriesId,ProductName,ProductDescription,Price,Inventory")] Product product, [Bind("SpecName")] string[] specName, List<IFormFile> file)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                if (file != null && file.Count > 0)
                {
                    int index = 1;
                    foreach (var files in file)
                    {
                        string uploadFolder = Path.Combine("wwwroot", "images");
                        string id = product.ProdcutId;
                        string folderPath = Path.Combine(uploadFolder, product.ProdcutId);


                        Directory.CreateDirectory(folderPath);

                        string fileName = index.ToString() + ".png";
                        index++;
                        string filePath = Path.Combine(folderPath, fileName);

                        using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await files.CopyToAsync(fileStream);
                        }
                    }

                }
                if (specName != null && specName.Length > 0)
                {
                    foreach(var spec in specName)
                    {
                        Spec specValue = new Spec{ SpecName = spec };
                        _context.Add(specValue);
                        await _context.SaveChangesAsync();
                        
                        ProductSpec ps = new ProductSpec { ProdcutId = product.ProdcutId, SpecId = specValue.SpecId};
                        _context.Add(ps);
                        _context.SaveChanges();
                    }
                }
                

                return RedirectToAction("ProductList");
            }
            ViewData["CategoriesID"] = new SelectList(_context.Categories, "CategoryID", "CategoryId", product.CategoriesId);
            return View(product);
        }
        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.ProdcutId == id);
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> Search(string? keyword)
        {
            List<Product> product = await _context.Products.Where(p => p.ProductName.Contains(keyword)).ToListAsync();
            ViewData["keyword"] = keyword;
            
            return View(product);
        }

        // 主類別
        public IActionResult MainCategory(string name)
        {
            var products = _context.Products
                .Where(p => p.Categories.FatherCategory.CategoryName == name)
                .ToList();
            
            ViewData["Name"] = name;
            ViewData["MainCategory"] = _context.Categories.Where(c => c.FatherCategory.CategoryName == name);
            return View(products);
        }

        // 子類
        public IActionResult Subcategory(string name) 
        {
            var products = _context.Products.Where(p => p.Categories.CategoryName == name);
            ViewData["Name"] = name;
            return View(products);
        }


        public JsonResult GetProduct(string id)
        {
            var products = _context.Products.Where(p => p.CategoriesId == id).ToList();

            return Json(products);
        }
    }
}
