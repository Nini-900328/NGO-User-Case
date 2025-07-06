using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NGOPlatformWeb.Models.Entity;      // User, Case, CaseLogin, NGODbContext
using NGOPlatformWeb.Models.ViewModels; // LoginViewModel
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NGOPlatformWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly NGODbContext _context;

        public AuthController(NGODbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // 1) 嘗試普通使用者
            var user = _context.Users
                .FirstOrDefault(u => u.Email == vm.Email && u.Password == vm.Password);
            if (user != null)
            {
                await SignInAsync(
                    id: user.UserId.ToString(),
                    name: user.Name,
                    role: "User"
                );

                // 民眾登入後導回首頁
                return RedirectToAction("Index", "Home");
            }

            // 2) 嘗試個案登入
            var caseLogin = _context.CaseLogins
                .FirstOrDefault(c => c.Email == vm.Email && c.Password == vm.Password);
            if (caseLogin != null)
            {
                // 取出對應 Case 主檔以取得名稱
                var cas = _context.Cases.Find(caseLogin.CaseId);
                var caseName = cas?.Name ?? "個案";

                await SignInAsync(
                    id: caseLogin.CaseId.ToString(),
                    name: caseName,
                    role: "Case"
                );

                // 個案登入後導向首頁
                return RedirectToAction("Index", "Home");
            }

            // 3) 都找不到就回錯誤
            ModelState.AddModelError(string.Empty, "帳號或密碼錯誤");
            return View(vm);
        }

        // POST: /Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        /// <summary>
        /// 統一建立 Cookie Authentication
        /// </summary>
        private Task SignInAsync(string id, string name, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name,           name),
                new Claim(ClaimTypes.Role,           role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            return HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true }
            );
        }
    }
}
