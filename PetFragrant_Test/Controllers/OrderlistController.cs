using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetFragrant_Test.ViewModels;
using PetFragrant_Test.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ECPay.Payment.Integration;
using System;
using PetFragrant_Test.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Security.Claims;
using PetFragrant_Test.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;
// using Microsoft.AspNetCore.Mvc;

namespace PetFragrant_Test.Controllers
{

    public class OrderlistController : Controller
    {
        private readonly PetContext _ctx;
        private readonly IMemoryCache _memoryCache;

        public OrderlistController(PetContext ctx, IMemoryCache memoryCache)
        {

            _ctx = ctx;
            _memoryCache = memoryCache;
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

        // 獲取我的訂單
        [Authorize]
        public IActionResult MyOrder()
        {
            var orders = _ctx.Orders
                .Include(o => o.OrderDetails).
                    ThenInclude(od => od.Prodcut).ThenInclude(p => p.ProductSpecs).ThenInclude(ps => ps.Spec).
                Where(o => o.CustomerId == UserID()).OrderByDescending(o => o.Orderdate);


            List<OrderDetailViewModel> odVM = new List<OrderDetailViewModel>();
            // 遍歷訂單並獲取相應的 TradeInfo
            foreach (var order in orders)
            {
                Discount discount = new Discount();
                decimal totolPrice = 0;
                if (order.CouponID != null)
                {
                    discount = _ctx.Discounts.Find(order.CouponID);
                    if(discount != null)
                    {
                        switch (discount.DiscountType)
                        {
                            case "折抵":
                                totolPrice = order.OrderDetails.Sum(od => Math.Round(od.Prodcut.Price * (decimal)0.9) * od.Amount) - discount.DiscountValue;
                                break;
                            case "折扣(%)":
                                totolPrice = order.OrderDetails.Sum(od => Math.Round(od.Prodcut.Price * (decimal)0.9) * od.Amount) * discount.DiscountValue;
                                break;

                        }
                    }

                }
                else
                {
                    totolPrice = order.OrderDetails.Sum(od => Math.Round(od.Prodcut.Price * (decimal)0.9) * od.Amount);
                }

                // 將 TradeInfo 加入對應的 ViewModel
                OrderDetailViewModel viewModel = new OrderDetailViewModel
                {
                    OrderId = order.OrderId,
                    Orderdate = order.Orderdate,
                    Shipdate = order.Shipdate,
                    Arriiveddate = order.Arriiveddate,
                    PaymentDate = order.Paymentdate,
                    Check = order.Check,
                    Status = order.status,
                    Payment = order.Payment,
                    Delivery = order.Delivery,
                    CouponName = discount != null ? discount.DiscoutName : string.Empty,
                    TotalPrice = totolPrice,
                    orderproduct = order.OrderDetails.Select(od => new OrderProductViewModel
                    {
                        ProdcutId = od.ProdcutId,
                        ProductName = od.Prodcut.ProductName,
                        SpecName = od.Prodcut.ProductSpecs.Select(ps => ps.Spec.SpecName).FirstOrDefault(),
                        Price = Math.Round(od.Prodcut.Price * (decimal)0.9),
                        PriceTotal = Math.Round(od.Prodcut.Price * (decimal)0.9) * od.Amount,
                        Amount = od.Amount
                    })
                };

                odVM.Add(viewModel);
            }

            return View(odVM);
        }
        // 購買紀錄
        [Authorize]
        public IActionResult History()
        {
            return View();
        }

        // 訂單確認
        [Authorize]
        [HttpPost]
        public IActionResult Check(List<OrderlistViewModel> items, string couponID)
        {
            Discount coupon = _ctx.Discounts.Find(couponID);
            if (coupon != null)
            {
                CouponViewModel couponVM = new CouponViewModel()
                {
                    ID = coupon.DiscoutID,
                    Name = coupon.DiscoutName,
                    Description = coupon.Description,
                    Value = coupon.DiscountValue,
                    Period = coupon.Period,
                    Type = coupon.DiscountType
                };
                ViewData["coupon"] = couponVM;
            }

           
            decimal total = 0;
            List<OrderCheckViewModel> orderCheck = new List<OrderCheckViewModel>();

            foreach (var item in items.Where(item => item.check))
            {
                var data = _ctx.ShoppingCarts
                    .Include(p => p.Prodcut)
                    .Include(p => p.Spec)
                    .FirstOrDefault(p => p.ProdcutId == item.ProductId && p.SpecId == item.oldspecID && p.CustomerId == UserID());

                if (data != null)
                {
                    ShoppingCart c = _ctx.ShoppingCarts.FirstOrDefault(p => p.ProdcutId == item.ProductId && p.SpecId == item.oldspecID && p.CustomerId == UserID());

                    if (c != null)
                    {
                        _ctx.Remove(c);
                        _ctx.SaveChanges();
                        ShoppingCart newcart = new ShoppingCart()
                        {
                            CustomerId = UserID(),
                            ProdcutId = item.ProductId,
                            SpecId = item.specID,
                            Quantity = item.quantity
                        };
                        _ctx.Add(newcart);
                        _ctx.SaveChanges();
                        data = _ctx.ShoppingCarts
                            .Include(p => p.Prodcut)
                            .Include(p => p.Spec)
                            .FirstOrDefault(p => p.ProdcutId == item.ProductId && p.SpecId == item.specID && p.CustomerId == UserID());
                    }

                    decimal price = Math.Round(data.Prodcut.Price * (decimal)0.9);

                    if (coupon?.DiscountType == "折扣(%)")
                    {
                        price = Math.Round(price * coupon.DiscountValue);
                    }

                    total += item.quantity * price;

                    orderCheck.Add(new OrderCheckViewModel
                    {
                        ProductID = item.ProductId,
                        ProductName = data.Prodcut.ProductName,
                        SpecID = data.SpecId,
                        SpecName = data.Spec.SpecName,
                        Amount = item.quantity,
                        Price = price
                    });
                }
            }

            if (coupon?.DiscountType == "折抵")
            {
                total -= coupon.DiscountValue;
            }

            
            var stores = _ctx.ReciveStores;
            List<StoreViewModel> storeVM = new List<StoreViewModel>();
            foreach(var store in stores)
            {
                storeVM.Add(new StoreViewModel()
                {
                    Id = store.Id,
                    Name = store.Name,
                    Address = store.Address
                });
            }
            ViewData["store"] = storeVM;
            ViewData["totalPrice"] = total;
            return View(orderCheck);
        }

        private string GenerateJwtToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // 生成金鑰
            byte[] keyBytes = new byte[16]; // 16 bytes = 128 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            string key = Convert.ToBase64String(keyBytes);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("UserID", userId) }),
                Expires = DateTime.UtcNow.AddHours(1), // Token 的有效期限
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        // 訂單成立
        [Authorize]
        public IActionResult FinishOrder()
        {
            return View();
        }


        public IActionResult CreateOrder()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult CreateOrder(List<OrderCheckViewModel> items, decimal totalprice, string CouponID, string storeID)
        {
            List<AllInOneResult> enErrors = new List<AllInOneResult>();

            try
            {
                // 新增order
                Order myorder = new Order
                {
                    OrderId = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999).ToString(),
                    Orderdate = DateTime.Now,
                    CustomerId = UserID(),
                    Payment = "",
                    Delivery = "",
                    Check = false,
                    StoreID = storeID,
                    CouponID = CouponID,
                    status = "未確認",
                    
                };
                _ctx.Add(myorder);
                _ctx.SaveChanges();

                // 建立 OD
                using (var transaction = _ctx.Database.BeginTransaction())
                {
                    try
                    {
                        List<OrderDetail> details = new List<OrderDetail>();
                        foreach (var item in items)
                        {
                            // 新增訂單細節到列表
                            details.Add(new OrderDetail
                            {
                                OrderId = myorder.OrderId,
                                ProdcutId = item.ProductID,
                                Amount = item.Amount,
                                QtDiscount = 0
                            });

                            // 減掉庫存
                            Product p = _ctx.Products.Find(item.ProductID);
                            p.Inventory -= item.Amount;

                            // 從購物車移除以購買的商品
                            ShoppingCart s = _ctx.ShoppingCarts.First(s => s.ProdcutId == item.ProductID && s.CustomerId == UserID() && s.SpecId == item.SpecID);
                            _ctx.Update(p);
                            _ctx.Remove(s);
                        }

                        // 批量新增訂單細節
                        _ctx.AddRange(details);
                        _ctx.SaveChanges();

                        // 提交交易
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // 發生錯誤時回滾交易
                        transaction.Rollback();
                        // 可以處理或記錄錯誤訊息
                        throw;
                    }
                }

                using (AllInOne oPayment = new AllInOne())
                {
                    /* 服務參數 */
                    oPayment.ServiceMethod = ECPay.Payment.Integration.HttpMethod.HttpPOST;
                    oPayment.ServiceURL = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";
                    oPayment.HashKey = "5294y06JbISpM5x9";
                    oPayment.HashIV = "v77hoKGq4kWxNNIS";
                    oPayment.MerchantID = "2000132";

                    /* 基本參數 */
                    oPayment.Send.MerchantTradeNo = myorder.OrderId;// 廠商的交易編號
                    //oPayment.Send.ReturnURL = "https://petfragrant.azurewebsites.net/Orderlist/ProcessFormData";  // 付款完成通知回傳的網址
                    //oPayment.Send.ClientBackURL = "https://petfragrant.azurewebsites.net/GetInfo/" + oPayment.Send.MerchantTradeNo; // 瀏覽器端返回的廠商網址
                    //oPayment.Send.OrderResultURL = "https://petfragrant.azurewebsites.net/Orderlist/GetInfo/" + oPayment.Send.MerchantTradeNo; // 瀏覽器端回傳付款結果網址
                    oPayment.Send.ReturnURL = "https://localhost:44383/Orderlist/GetInfo/" + oPayment.Send.MerchantTradeNo; // 付款完成通知回傳的網址
                    oPayment.Send.ClientBackURL = "https://localhost:44383/Orderlist/GetInfo/" + oPayment.Send.MerchantTradeNo; // 瀏覽器端返回的廠商網址
                    oPayment.Send.OrderResultURL = "https://localhost:44383/Orderlist/GetInfo/" + oPayment.Send.MerchantTradeNo; // 瀏覽器端回傳付款結果網址

                    oPayment.Send.MerchantTradeDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"); // 廠商的交易時間
                    oPayment.Send.ChoosePayment = PaymentMethod.ALL; // 使用的付款方式
                    oPayment.Send.NeedExtraPaidInfo = ExtraPaymentInfo.No; // 是否需要額外的付款資訊
                    oPayment.Send.DeviceSource = DeviceType.PC; // 來源裝置
                    oPayment.Send.HoldTradeAMT = HoldTradeType.Yes;
                    oPayment.Send.EncryptType = 1;
                    oPayment.Send.TotalAmount = (decimal)Math.Round(totalprice);
                    oPayment.Send.TradeDesc = "購買" + items.Count() + "個商品，總價為NT " + totalprice;
                    oPayment.Send.Remark = "";
                    oPayment.Send.CustomField1 = "";
                    oPayment.Send.CustomField2 = "";
                    oPayment.Send.CustomField3 = "";
                    oPayment.Send.CustomField4 = "";

                    foreach (var item in items)
                    {
                        oPayment.Send.Items.Add(new Item()
                        {
                            Name = item.ProductName + "-" + item.SpecName,
                            Price = item.Price,
                            Currency = "新台幣",
                            Quantity = item.Amount,
                            URL = "",
                        });
                    }

                    AllInOneResult result = oPayment.CheckOut();
                    enErrors.Add(result);
                }
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException;
                throw;
            }
            finally
            {
                if (enErrors.Count() > 0)
                {
                    string szErrorMessage = string.Join("\\r\\n", enErrors.Select(e => e.ToString()));
                    ViewBag.ErrorMessage = szErrorMessage;
                }
            }

            return View(enErrors);
        }

        // 取得付款資訊
        public ActionResult GetInfo(string id)
        {
            List<string> enErrors = new List<string>();
            Hashtable htFeedback = null;
            var data = _ctx.Orders.Find(id);
            if (data != null)
            {
                try
                {
                    using (AllInOne oPayment = new AllInOne())
                    {
                        /* 服務參數 */
                        oPayment.ServiceMethod = ECPay.Payment.Integration.HttpMethod.ServerPOST;
                        oPayment.ServiceURL = "https://payment-stage.ecpay.com.tw/Cashier/QueryTradeInfo/V5";
                        oPayment.HashKey = "5294y06JbISpM5x9";
                        oPayment.HashIV = "v77hoKGq4kWxNNIS";
                        oPayment.MerchantID = "2000132";

                        /* 基本參數 */
                        oPayment.Query.MerchantTradeNo = id;


                        /* 查詢訂單 */
                        enErrors.AddRange(oPayment.QueryTradeInfo(ref htFeedback));
                    }

                    // 取回所有資料
                    if (enErrors.Count == 0)
                    {
                        TradeInfo tradeInfo = new TradeInfo();

                        // 取得資料
                        foreach (string szKey in htFeedback.Keys)
                        {
                            switch (szKey)
                            {
                                /* 支付後的回傳的基本參數 */
                                case "MerchantID":
                                    tradeInfo.MerchantID = htFeedback[szKey].ToString();
                                    break;
                                case "MerchantTradeNo":
                                    tradeInfo.MerchantTradeNo = htFeedback[szKey].ToString();
                                    break;
                                case "PaymentDate":
                                    tradeInfo.PaymentDate = htFeedback[szKey].ToString();
                                    break;
                                case "PaymentType":
                                    tradeInfo.PaymentType = htFeedback[szKey].ToString();
                                    break;
                                case "PaymentTypeChargeFee":
                                    tradeInfo.PaymentTypeChargeFee = htFeedback[szKey].ToString();
                                    break;
                                case "RtnCode":
                                    tradeInfo.RtnCode = htFeedback[szKey].ToString();
                                    break;
                                case "RtnMsg":
                                    tradeInfo.RtnMsg = htFeedback[szKey].ToString();
                                    break;
                                case "SimulatePaid":
                                    tradeInfo.SimulatePaid = htFeedback[szKey].ToString();
                                    break;
                                case "TradeAmt":
                                    tradeInfo.TradeAmt = htFeedback[szKey].ToString();
                                    break;
                                case "TradeDate":
                                    tradeInfo.TradeDate = htFeedback[szKey].ToString();
                                    break;
                                case "TradeNo":
                                    tradeInfo.TradeNo = htFeedback[szKey].ToString();
                                    break;
                                default:
                                    break;
                            }
                        }
                        if(tradeInfo.PaymentDate != "")
                        {
                            data.Payment = tradeInfo.PaymentType;
                            data.Paymentdate = DateTime.Parse(tradeInfo.PaymentDate);
                            _ctx.Update(data);
                            _ctx.SaveChanges();
                        }

                        return View(tradeInfo);
                    }
                    else
                    {

                        return NotFound();// 回覆錯誤訊息
                    }
                }
                catch (Exception ex)
                {
                    // 例外錯誤處理
                    enErrors.Add(ex.Message);
                    return Content(ex.Message);// 回覆錯誤訊息
                }
            }
            else
            {
                return Content("NOT FOUND!!!!");
            }

            //return View();
        }

        [HttpPost]
        public IActionResult ProcessFormData(IFormCollection formCollection)
        {
            // 在這裡處理接收到的 Form Data
            // ...

            // 回傳 1|OK
            return Content("1|OK");
        }
        public IActionResult QueryOrder()
        {
            
            return View();
        }


    }
}
