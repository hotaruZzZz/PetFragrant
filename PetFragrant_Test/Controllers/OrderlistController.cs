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

namespace PetFragrant_Test.Controllers
{
    public class OrderlistController : Controller
    {
        private readonly PetContext _ctx;
        public OrderlistController(PetContext ctx)
        {
            _ctx = ctx;
        }

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
            List<ShoppingCartViewModel> cart = new List<ShoppingCartViewModel>();
            decimal total = 0;
            foreach (var item in items)
            {
                if (item.check)
                {
                    var data = _ctx.ShoppingCarts
                        .Include(p => p.Prodcut)
                        .Include(p => p.Spec)
                        .FirstOrDefault(p => p.SpecId == item.specID && p.ProdcutId == item.ProductId);
                    data.Quantity = item.quantity;
                    total += data.Quantity * data.Prodcut.Price;
                    if (data != null)
                    {
                        cart.Add(new ShoppingCartViewModel
                        {
                            ProductData = new ProductViewModel
                            {
                                ProductData = data.Prodcut,
                            },
                            ShoppingCartData = data
                        });
                    }
                }
            }
            ViewData["totalPrice"] = total;
            return View(cart);
        }

        [Authorize]
        public IActionResult FinishOrder()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult CreateOrder(List<OrderlistViewModel> items)
        {
            List<AllInOneResult> enErrors = new List<AllInOneResult>();

            try
            {
                List<ShoppingCartViewModel> cart = new List<ShoppingCartViewModel>();
                foreach (var item in items)
                {
                    if (item.check) { 
                        var data = _ctx.ShoppingCarts
                        .Include(p => p.Prodcut)
                        .Include(p => p.Spec)
                        .FirstOrDefault(p => p.SpecId == item.specID && p.ProdcutId == item.ProductId);

                    if (data != null)
                    {
                        cart.Add(new ShoppingCartViewModel
                        {
                            ProductData = new ProductViewModel
                            {
                                ProductData = data.Prodcut,
                            },
                            ShoppingCartData = data
                        });
                    }
                    }
                }

                using (AllInOne oPayment = new AllInOne())
                {
                    /* 服務參數 */
                    oPayment.ServiceMethod = HttpMethod.HttpPOST;
                    oPayment.ServiceURL = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";
                    oPayment.HashKey = "5294y06JbISpM5x9";
                    oPayment.HashIV = "v77hoKGq4kWxNNIS";
                    oPayment.MerchantID = "2000132";

                    /* 基本參數 */
                    oPayment.Send.ReturnURL = "https://localhost:44383/ShoppingCart";//付款完成通知回傳的網址
                    oPayment.Send.ClientBackURL = "https://localhost:44383/ShoppingCart";//瀏覽器端返回的廠商網址
                    oPayment.Send.OrderResultURL = "https://localhost:44383/ShoppingCart";//瀏覽器端回傳付款結果網址
                    oPayment.Send.MerchantTradeDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");//廠商的交易時間
                    oPayment.Send.ChoosePayment = PaymentMethod.ALL;//使用的付款方式
                    oPayment.Send.NeedExtraPaidInfo = ExtraPaymentInfo.No;//是否需要額外的付款資訊
                    oPayment.Send.DeviceSource = DeviceType.PC;//來源裝置
                    oPayment.Send.HoldTradeAMT = HoldTradeType.Yes;
                    oPayment.Send.EncryptType = 1;
                    oPayment.Send.MerchantTradeNo = "ECPay" + new Random().Next(0, 99999).ToString();//廠商的交易編號
                    oPayment.Send.TotalAmount = (Decimal)Math.Round(cart.Sum(item => item.ShoppingCartData.Prodcut.Price*item.ShoppingCartData.Quantity));
                    oPayment.Send.TradeDesc = "交易描述";
                    oPayment.Send.Remark = "";
                    oPayment.Send.CustomField1 = "";
                    oPayment.Send.CustomField2 = "";
                    oPayment.Send.CustomField3 = "";
                    oPayment.Send.CustomField4 = "";

                    foreach (var item in cart)
                    {
                        oPayment.Send.Items.Add(new Item()
                        {
                            Name = item.ShoppingCartData.Prodcut.ProductName + "-" + item.ShoppingCartData.Spec.SpecName,
                            Price = item.ShoppingCartData.Prodcut.Price,
                            Currency = "新台幣",
                            Quantity = item.ShoppingCartData.Quantity,
                            URL = "http://google.com",
                        });
                    }


                    AllInOneResult result = oPayment.CheckOut();
                        enErrors.Add(result);

                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
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



    }
}
