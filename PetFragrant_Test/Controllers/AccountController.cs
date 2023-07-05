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

namespace PetFragrant_Test.Controllers
{
    public class AccountController : Controller
    {
        private readonly PetContext _ctx;
        private readonly IHashService _hashService;

        public AccountController(PetContext ctx, IHashService hashService)
        {
            _ctx = ctx;
            _hashService = hashService;
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

            return LocalRedirect("~/Home");
            }
            return View(logvVM);
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
                    IsAdmin = user.IsAdmin
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
                    Level = "一般會員"
                };

                _ctx.Customers.Add(user);
                _ctx.SaveChanges();

                ViewData["Title"] = "帳號註冊";
                ViewData["Message"] = "使用者帳號註冊成功!";  //顯示訊息

                return View("~/Views/Shared/ResultMessage.cshtml");

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

            GoogleJsonWebSignature.Payload? payload = VerifyGoogleToken(formCredential, formToken, cookiesToken).Result;

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

        public IActionResult SendMessage()
        {
            return View();
        }
    }
}
