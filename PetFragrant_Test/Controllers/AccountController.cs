using Microsoft.AspNetCore.Mvc;
using PetFragrant_Test.Data;
using PetFragrant_Test.Interface;
using PetFragrant_Test.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using System;
using CoreMvc5_CookieAuthentication.ViewModels;
using PetFragrant_Test.Models;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace PetFragrant_Test.Controllers
{
    public class AccountController : Controller
    {
        private readonly PetContext _ctx;
        private readonly IHashService _hashService;
        private readonly IConfiguration _config;

        public AccountController(PetContext ctx, IHashService hashService, IConfiguration config)
        {
            _ctx = ctx;
            _hashService = hashService;
            _config = config;

        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel logvVM)
        {
            if(ModelState.IsValid) { 
            
            ApplicationUser user = await AuthenticateUser(logvVM);

            // fail
            if(user == null)
            {
                ModelState.AddModelError(string.Empty, "帳號或密碼有誤");
                return View(logvVM);
            }
            if(user.EmailConfirmed == false)
                {
                    return RedirectToAction("SendMessage", new { user = JsonConvert.SerializeObject(user) });
                }
            
            //成功，通過帳比對，以下開始建立授權
            var claims = new List<Claim>
                {
                    new Claim("UserID", user.UserID, ClaimValueTypes.String),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim("IsAdmin", user.IsAdmin.ToString(), ClaimValueTypes.Boolean )
                };
            
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
          
            };
            await HttpContext.SignInAsync(
                   CookieAuthenticationDefaults.AuthenticationScheme,
                   new ClaimsPrincipal(claimsIdentity),
                   authProperties
                   );
                bool isadmin = Convert.ToBoolean( claims.FirstOrDefault(c => c.Type == "IsAdmin").Value);
                if (isadmin)
                {
                    return Redirect("~/Backstage");
                }
                else
                {
                    return LocalRedirect("~/Home");
                }
            
            }
            return View(logvVM);
        }

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
        private async Task<ApplicationUser> AuthenticateUser(LoginViewModel loginVM)
        {
            var user = await _ctx.Customers
                .FirstOrDefaultAsync(u => u.Email == loginVM.UserEmail &&  u.Password == _hashService.MD5Hash(loginVM.Password));
               
            if (user != null)
            {
                

                var userInfo = new ApplicationUser
                {
                    UserID = user.CustomerId,
                    Name = user.CustomerName,
                    Email = user.Email,
                    PhoneNo = user.PhoneNumber,
                    IsAdmin = user.IsAdmin,
                    EmailConfirmed = (bool)user.EmailConfirmed
                };
                return userInfo;
            }
            else
            {
                return null;
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel registerVM)
        {
            if (ModelState.IsValid)
            {
                Customer user = new Customer
                {
                    CustomerId = Guid.NewGuid().ToString(),
                    CustomerName = registerVM.UserName,
                    Password = _hashService.MD5Hash(registerVM.Password),
                    PhoneNumber = registerVM.PhoneNumber,
                    Email = registerVM.Email,
                    Birthday = registerVM.Birthdate,
                    Address = registerVM.Address,
                    IsAdmin = false,
                    EmailConfirmed = false,
                    Level = "一般會員"
                };

                _ctx.Customers.Add(user);
                _ctx.SaveChanges();
                ApplicationUser applicationUser = new ApplicationUser
                {
                    UserID = user.CustomerId,
                    Name = user.CustomerName,
                    Email = user.Email,
                    PhoneNo = user.PhoneNumber,
                    IsAdmin = user.IsAdmin
                };
               

                return RedirectToAction("SendMessage", new { user = JsonConvert.SerializeObject(applicationUser) });


            }
            return View();
        }

        //登出
        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/");
        }

        public IActionResult Forbidden()
        {
            return View();
        }


        // google login
        public IActionResult ValidGoogleLogin()
        {
            string? formCredential = Request.Form["credential"]; // 回傳憑證
            string? formToken = Request.Form["g_csrf_token"]; // 回傳 token
            string? cookiesToken = Request.Cookies["g_csrf_token"]; // cookie token

            GoogleJsonWebSignature.Payload payload = VerifyGoogleToken(formCredential, formToken, cookiesToken).Result;

            if(payload == null)
            {
                return Content("驗證Google授權失敗");
            }
            else
            {
                ViewData["Msg"] = "驗證成功!";
            }

            return View();

        }

        public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(string? formCredential, string? formToken, string? cookiesToken)
        {
            // 檢查空值
            if (formCredential == null || formToken == null && cookiesToken == null)
            {
                return null;
            }

            GoogleJsonWebSignature.Payload? payload;
            try
            {
                // 驗證 token
                if (formToken != cookiesToken)
                {
                    return null;
                }

                // 驗證憑證
                IConfiguration Config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
                string GoogleApiClientId = Config.GetSection("GoogleApiClientId").Value;
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { GoogleApiClientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(formCredential, settings);
                if (!payload.Issuer.Equals("accounts.google.com") && !payload.Issuer.Equals("https://accounts.google.com"))
                {
                    return null;
                }
                if (payload.ExpirationTimeSeconds == null)
                {
                    return null;
                }
                else
                {
                    DateTime now = DateTime.Now.ToUniversalTime();
                    DateTime expiration = DateTimeOffset.FromUnixTimeSeconds((long)payload.ExpirationTimeSeconds).DateTime;
                    if (now > expiration)
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
            return payload;
        }

        [HttpGet]
        public IActionResult SendMessage(string user)
        {
            ApplicationUser applicationUser = JsonConvert.DeserializeObject<ApplicationUser>(user);
            string GoogleID = "petsfragrant@gmail.com";
            string TempPwd = _config["TempPwd"];
            string ReceiveMail = applicationUser.Email;

            string SmtpServer = "smtp.gmail.com";
            string vcode = _hashService.MD5Hash(applicationUser.Email);
            int SmtpPort = 587;
            MailMessage mss = new MailMessage();
            mss.From = new MailAddress(GoogleID);
            mss.Subject = "寵物香園信箱驗證信";
            mss.Body = "<p>\n    親愛的" + applicationUser.Name + "， 感謝您註冊成為「寵物香園」的會員！為了確保您的郵件地址正確並驗證您的帳戶，請點擊以下連結完成驗證程序：<br /> \n    <a href=\"#\">點擊此處驗證郵件地址 </a>\n    這個連結將會帶您到我們的網站，驗證您的郵件地址並啟用您的會員帳戶。如果上述連結無效，您也可以在瀏覽器中輸入以下驗證碼進行驗證：\n    <br /><span style=\"background-color:#FFD54A;color:#12130F;\">" + vcode +
                "</span><br />\n     謝謝您的合作！如有任何疑問或需要協助，請隨時聯繫我們的客戶服務團隊。 祝您有愉快的購物體驗！ 寵物香園團隊\n</p>\n";
            mss.IsBodyHtml = true;
            mss.SubjectEncoding = Encoding.UTF8;
            mss.To.Add(new MailAddress(ReceiveMail));
            using (SmtpClient client = new SmtpClient(SmtpServer, SmtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(GoogleID, TempPwd);
                client.Send(mss);
            }
            ViewData["ErrorMessage"] = "";
            ViewData["Email"] = ReceiveMail;
            return View();
        }
        [HttpPost]
        public IActionResult Verification(string code, string email)
        {
            ViewData["Title"] = "帳號註冊";
            if (ModelState.IsValid)
            {
                var user = _ctx.Customers.First(c => c.Email == email);
                if (user != null)
                {
                    if (code == _hashService.MD5Hash(user.Email))
                    {

                        user.EmailConfirmed = true;
                        _ctx.Update(user);
                        _ctx.SaveChanges();
                        ViewData["ResultMessage"] = "建立使用者成功!";  //顯示訊息
                        ViewData["RedirectUrl"] = "/Home/Index";       //跳轉頁網址
                        ViewData["RedirectTime"] = 6; //倒數幾秒
                        ViewData["Message"] = "使用者帳號註冊成功!";  //顯示訊息
                        return View("~/Views/Shared/ResultMessage.cshtml");
                    }
                    else
                    {
                        ViewData["ErrorMessage"] = "驗證碼錯誤，請重新輸入。";
                        return Redirect("Home");
                    }
                }
                return View();
            }
            else
            {
                return NotFound();
            }
        }

    }
}
